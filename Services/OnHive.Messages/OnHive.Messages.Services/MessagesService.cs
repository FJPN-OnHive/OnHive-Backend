using AutoMapper;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Validations.Common;
using OnHive.Core.Library.Entities.Messages;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Messages.Domain.Abstractions.Repositories;
using OnHive.Messages.Domain.Abstractions.Services;
using OnHive.Messages.Domain.Models;
using Serilog;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Enums.Messages;
using OnHive.Core.Library.Helpers;
using System.Text.Json;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Emails.Domain.Abstractions.Services;
namespace OnHive.Messages.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly IMessagesRepository messagesRepository;
        private readonly IMessageChannelRepository messageChannelsRepository;
        private readonly IMessageUsersRepository messageUsersRepository;
        private readonly IUserGroupsService userGroupsService;
        private readonly MessagesApiSettings messagesApiSettings;
        private readonly IUsersService usersService;
        private readonly IEmailsService emailsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public MessagesService(IMessagesRepository messagesRepository,
                               IMessageChannelRepository messageChannelsRepository,
                               IMessageUsersRepository messageUsersRepository,
                               MessagesApiSettings messagesApiSettings,
                               IMapper mapper,
                               IUsersService usersService,
                               IUserGroupsService userGroupsService,
                               IEmailsService emailsService)
        {
            this.messagesRepository = messagesRepository;
            this.messageChannelsRepository = messageChannelsRepository;
            this.messageUsersRepository = messageUsersRepository;
            this.messagesApiSettings = messagesApiSettings;
            this.usersService = usersService;
            this.userGroupsService = userGroupsService;
            this.emailsService = emailsService;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<List<MessageUserDto>> GetListByUserAsync(LoggedUserDto loggedUser, bool newOnly)
        {
            var userMessages = await messageUsersRepository.GetByUserAsync(loggedUser.User.Id, newOnly, loggedUser.User.TenantId);
            return mapper.Map<List<MessageUserDto>>(userMessages); ;
        }

        public async Task<MessageDto?> GetByIdAsync(string messageId, LoggedUserDto loggedUser)
        {
            var message = await messagesRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                return null;
            }
            await ValidateUserPermission(messageId, loggedUser);
            return mapper.Map<MessageDto>(message);
        }

        public async Task<PaginatedResult<MessageDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await messagesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<MessageDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<MessageDto>>(result.Itens)
                };
            }
            return new PaginatedResult<MessageDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<MessageDto>()
            };
        }

        public async Task<IEnumerable<MessageDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var messages = await messagesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<MessageDto> SaveAsync(MessageDto messageDto, LoggedUserDto? loggedUser)
        {
            var message = mapper.Map<Message>(messageDto);
            ValidatePermissions(message, loggedUser?.User);
            message.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            message.CreatedAt = DateTime.UtcNow;
            message.CreatedBy = string.IsNullOrEmpty(message.CreatedBy) ? loggedUser?.User?.Id : message.CreatedBy;
            var response = await messagesRepository.SaveAsync(message);
            return mapper.Map<MessageDto>(response);
        }

        public async Task<MessageDto> CreateAsync(MessageDto messageDto, LoggedUserDto? loggedUser)
        {
            if (!messageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var message = mapper.Map<Message>(messageDto);
            ValidatePermissions(message, loggedUser?.User);
            message.Id = string.Empty;
            message.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            message.IsActive = true;
            var response = await messagesRepository.SaveAsync(message, loggedUser.User.Id);
            return mapper.Map<MessageDto>(response);
        }

        public async Task<MessageDto?> UpdateAsync(MessageDto messageDto, LoggedUserDto? loggedUser)
        {
            if (!messageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var message = mapper.Map<Message>(messageDto);
            ValidatePermissions(message, loggedUser?.User);
            var currentMessage = await messagesRepository.GetByIdAsync(message.Id);
            if (currentMessage == null || currentMessage.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await messagesRepository.SaveAsync(message, loggedUser.User.Id);
            return mapper.Map<MessageDto>(response);
        }

        public async Task SendMessageAsync(MessageDto messageDto)
        {
            await VerifyMessageAsync(messageDto);
            messageDto.MessageDate = DateTime.Now;
            messageDto.Status = MessageStatus.New;
            var message = mapper.Map<Message>(messageDto);
            var channel = await messageChannelsRepository.GetByCodeAsync(messageDto.ChannelCode, messageDto.TenantId) ?? throw new NotFoundException("Channel not found");
            messageDto.ChannelId = channel.Id;
            var users = new List<MessageUser>();
            message = await messagesRepository.SaveAsync(message);
            foreach (var user in channel.UsersIds)
            {
                users.Add(new MessageUser
                {
                    UserId = user,
                    MessageId = message.Id,
                    TenantId = message.TenantId,
                    Status = MessageStatus.New,
                    MessageDate = DateTime.UtcNow,
                    From = message.From.Email,
                    Subject = message.Subject,
                });
            }
            foreach (var userGroup in channel.UsersGroupIds)
            {
                var usersGroup = await GetUserGroup(userGroup);
                if (usersGroup != null)
                {
                    foreach (var userId in usersGroup.UsersIds)
                    {
                        users.Add(new MessageUser
                        {
                            UserId = userId,
                            MessageId = message.Id,
                            TenantId = message.TenantId,
                            Status = MessageStatus.New,
                            MessageDate = DateTime.UtcNow,
                            From = message.From.Email,
                            Subject = message.Subject,
                        });
                    }
                }
            }
            foreach (var messageUser in users.DistinctBy(u => u.UserId))
            {
                await messageUsersRepository.SaveAsync(messageUser);
            }
            if (channel.SendEmail)
            {
                await SendEmailAsync(users, message, channel);
            }
        }

        public async Task<MessageDto> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var current = await messagesRepository.GetByIdAsync(patch.GetId());
            if (current == null || current.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            current = patch.PatchEntity(current);
            ValidatePermissions(current, loggedUser.User);
            await messagesRepository.SaveAsync(current);
            return mapper.Map<MessageDto>(current);
        }

        public async Task<MessageUserDto> PatchUserMessageAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var current = await messageUsersRepository.GetByIdAsync(patch.GetId());
            if (current == null || current.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            current = patch.PatchEntity(current);
            ValidateUserPermissions(current, loggedUser.User);
            await messageUsersRepository.SaveAsync(current);
            return mapper.Map<MessageUserDto>(current);
        }

        public async Task<MessageUserDto> UpdateUserMessageAsync(MessageUserDto messageDto, LoggedUserDto loggedUser)
        {
            if (!messageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var message = mapper.Map<MessageUser>(messageDto);
            ValidateUserPermissions(message, loggedUser?.User);
            var currentMessage = await messageUsersRepository.GetByIdAsync(message.Id);
            if (currentMessage == null || currentMessage.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await messageUsersRepository.SaveAsync(message, loggedUser.User.Id);
            return mapper.Map<MessageUserDto>(response);
        }

        private async Task ValidateUserPermission(string messageId, LoggedUserDto loggedUser)
        {
            var users = await messageUsersRepository.GetByMessageAsync(messageId, loggedUser.User.TenantId);
            if (!users.ToList().Exists(users => users.UserId == loggedUser.User.Id)
                   && !loggedUser.User.Permissions.Exists(p => p.Equals(messagesApiSettings.MessagesAdminPermission, StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException();
            }
        }

        private async Task SendEmailAsync(List<MessageUser> users, Message message, MessageChannel channel)
        {
            try
            {
                foreach (var user in users)
                {
                    var userDto = await GetUser(user.UserId);
                    if (userDto != null)
                    {
                        var email = new EmailSendDto
                        {
                            TenantId = message.TenantId,
                            SendTo = [userDto.MainEmail],
                            From = message.From.Email,
                            TemplateCode = channel.EmailTemplateCode,
                            Fields = new Dictionary<string, string>
                            {
                                { "message", message.Body },
                                { "channel", channel.Name },
                                { "name", message.From.Name },
                                { "phone", message.From.Phone },
                            }
                        };
                        await emailsService.ComposeEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error sending emails", ex);
            }
        }

        private async Task<UserDto?> GetUser(string userId)
        {
            var user = await usersService.GetByIdAsync(userId);
            if (user != null)
            {
                return user;
            }
            logger.Warning("User not found {userGroup}", userId);
            return null;
        }

        private async Task<UserGroupDto?> GetUserGroup(string userGroupId)
        {
            var userGroup = await userGroupsService.GetByIdAsync(userGroupId);
            if (userGroup != null)
            {
                return userGroup;
            }
            logger.Warning("UserGroup not found {userGroup}", userGroup);
            return null;
        }

        private async Task VerifyMessageAsync(MessageDto messageDto)
        {
            if (!messageDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var messages = await messagesRepository.GetByFromAsync(messageDto.From.Email, messageDto.Origin, messageDto.TenantId);
            if (messages.Exists(m => m.Body.Equals(messageDto.Body, StringComparison.InvariantCultureIgnoreCase)
                && Math.Abs((m.MessageDate - DateTime.Now).TotalHours) <= messagesApiSettings.DuplicatedMessageTimeCheckHours))
            {
                throw new DuplicatedException("Message already sent");
            }
        }

        private void ValidatePermissions(Message message, UserDto? loggedUser)
        {
            if (loggedUser != null && message.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Message/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    message.Id, message.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private void ValidateUserPermissions(MessageUser message, UserDto? loggedUser)
        {
            if (loggedUser != null && message.TenantId != loggedUser.TenantId && message.UserId != loggedUser.Id)
            {
                logger.Warning("Unauthorized update mismatch tenantID Message/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    message.Id, message.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}