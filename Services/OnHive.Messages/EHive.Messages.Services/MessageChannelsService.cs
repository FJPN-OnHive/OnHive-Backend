using AutoMapper;
using EHive.Core.Library.Contracts.Messages;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Messages;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Messages.Domain.Abstractions.Repositories;
using EHive.Messages.Domain.Abstractions.Services;
using EHive.Messages.Domain.Models;
using Serilog;
using EHive.Core.Library.Exceptions;

namespace EHive.Messages.Services
{
    public class MessageChannelsService : IMessageChannelsService
    {
        private readonly IMessageChannelRepository messageChannelsRepository;
        private readonly MessagesApiSettings messagesApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public MessageChannelsService(IMessageChannelRepository messageChannelsRepository, MessagesApiSettings messagesApiSettings, IMapper mapper)
        {
            this.messageChannelsRepository = messageChannelsRepository;
            this.messagesApiSettings = messagesApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<MessageChannelDto?> GetByIdAsync(string messageId)
        {
            var message = await messageChannelsRepository.GetByIdAsync(messageId);
            return mapper.Map<MessageChannelDto>(message);
        }

        public async Task<PaginatedResult<MessageChannelDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await messageChannelsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<MessageChannelDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<MessageChannelDto>>(result.Itens)
                };
            }
            return new PaginatedResult<MessageChannelDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<MessageChannelDto>()
            };
        }

        public async Task<List<string>> GetByTenant(RequestFilter filter, string tenantId)
        {
            var messages = await messageChannelsRepository.GetAllAsync(tenantId);
            if (messages == null || !messages.Any())
            {
                return new List<string>();
            }
            return messages.Select(m => m.Code).ToList();
        }

        public async Task<IEnumerable<MessageChannelDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var messages = await messageChannelsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<MessageChannelDto>>(messages);
        }

        public async Task<MessageChannelDto> SaveAsync(MessageChannelDto MessageChannelDto, LoggedUserDto? loggedUser)
        {
            var message = mapper.Map<MessageChannel>(MessageChannelDto);
            await VerifyDuplicate(message);
            ValidatePermissions(message, loggedUser?.User);
            message.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            message.CreatedAt = DateTime.UtcNow;
            message.CreatedBy = string.IsNullOrEmpty(message.CreatedBy) ? loggedUser?.User?.Id : message.CreatedBy;
            message.IsActive = true;
            var response = await messageChannelsRepository.SaveAsync(message);
            return mapper.Map<MessageChannelDto>(response);
        }

        public async Task<MessageChannelDto> CreateAsync(MessageChannelDto MessageChannelDto, LoggedUserDto? loggedUser)
        {
            if (!MessageChannelDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var message = mapper.Map<MessageChannel>(MessageChannelDto);
            await VerifyDuplicate(message);
            ValidatePermissions(message, loggedUser?.User);
            message.Id = string.Empty;
            message.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            message.IsActive = true;
            var response = await messageChannelsRepository.SaveAsync(message, loggedUser.User.Id);
            return mapper.Map<MessageChannelDto>(response);
        }

        public async Task<MessageChannelDto?> UpdateAsync(MessageChannelDto MessageChannelDto, LoggedUserDto? loggedUser)
        {
            if (!MessageChannelDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var message = mapper.Map<MessageChannel>(MessageChannelDto);
            message.IsActive = true;
            await VerifyDuplicate(message);
            ValidatePermissions(message, loggedUser?.User);
            var currentMessage = await messageChannelsRepository.GetByIdAsync(message.Id);
            if (currentMessage == null || currentMessage.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await messageChannelsRepository.SaveAsync(message, loggedUser.User.Id);
            return mapper.Map<MessageChannelDto>(response);
        }

        public async Task<bool> DeleteAsync(string messageId, LoggedUserDto loggedUser)
        {
            var currentMessage = await messageChannelsRepository.GetByIdAsync(messageId);
            if (currentMessage == null || currentMessage.TenantId != loggedUser.User.TenantId)
            {
                return false;
            }
            ValidatePermissions(currentMessage, loggedUser.User);
            await messageChannelsRepository.DeleteAsync(messageId);
            logger.Information("Message Channel {id} deleted by {userId}", messageId, loggedUser.User.Id);
            return true;
        }

        private async Task VerifyDuplicate(MessageChannel message)
        {
            var current = await messageChannelsRepository.GetByCodeAsync(message.Code, message.TenantId);
            if (current != null && !current.Id.Equals(message.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new DuplicatedException("Message Channel already exists");
            }
        }

        private void ValidatePermissions(MessageChannel messageChannel, UserDto? loggedUser)
        {
            if (loggedUser != null && messageChannel.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Message/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    messageChannel.Id, messageChannel.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}