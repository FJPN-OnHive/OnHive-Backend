using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Messages;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;

namespace OnHive.Admin.Pages
{
    public class MessageChannelsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<MessageChannelDto> Channels { get; set; } = [];

        public MessageChannelDto? SelectedChannel { get; set; } = new();

        public HashSet<UserDto> SelectedUsers { get; set; } = new();

        public List<UserDto> AllUsers { get; set; } = new();

        public HashSet<UserGroupDto> SelectedGroups { get; set; } = new();

        public List<UserGroupDto> AllGroups { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public bool ShowDeleteMessage { get; set; } = false;

        public string SearchChannel { get; set; } = string.Empty;

        public string SearchGroup { get; set; } = string.Empty;

        public string SearchUser { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/messageChannels");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/messageChannels");
            }
            await LoadGroups();
            Loading = false;
        }

        public async Task SaveChannel()
        {
            try
            {
                Message = string.Empty;
                if (SelectedChannel != null)
                {
                    SelectedChannel.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedChannel.UsersIds = SelectedUsers.Select(u => u.Id).ToList();
                    SelectedChannel.UsersGroupIds = SelectedGroups.Select(u => u.Id).ToList();
                    SelectedChannel = await AdminService.MessageChannelsService.Save(SelectedChannel, string.IsNullOrEmpty(SelectedChannel.Id), AdminService.LoggedUser.Token);
                    Message = "Canal salvo com sucesso";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messageChannels");
            }
            catch (DuplicatedException)
            {
                Message = $"O Canal {SelectedChannel?.Name ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task DeleteChannel()
        {
            try
            {
                Message = string.Empty;
                if (SelectedChannel != null)
                {
                    var result = await AdminService.MessageChannelsService.Delete(SelectedChannel.Id, AdminService.LoggedUser.Token);
                    if (result)
                    {
                        Channels.Remove(SelectedChannel);
                        SelectedChannel = new();
                        SelectedUsers = new();
                        SelectedGroups = new();
                        Message = "Canal deletado com sucesso";
                        ShowMessage = true;
                    }
                    else
                    {
                        Message = "Canal não encontrado";
                        ShowMessage = true;
                    }
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messageChannels");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SelectGroup(MessageChannelDto channel)
        {
            SelectedChannel = channel;
            SelectedUsers = AllUsers.Where(u => channel.UsersIds.Contains(u.Id)).Distinct().ToHashSet();
            SelectedGroups = AllGroups.Where(g => channel.UsersGroupIds.Contains(g.Id)).Distinct().ToHashSet();
            StateHasChanged();
        }

        public void Clear()
        {
            SelectedChannel = new();
            SelectedUsers = new();
            SelectedGroups = new();
            StateHasChanged();
        }

        public string RowClassSelector(MessageChannelDto item, int rowIndex)
        {
            return (item.Equals(SelectedChannel))
                ? "mud-info"
                : "";
        }

        public async Task TestMessage()
        {
            try
            {
                Message = string.Empty;
                if (SelectedChannel != null)
                {
                    var message = new MessageDto
                    {
                        TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty,
                        ChannelId = SelectedChannel.Id,
                        ChannelCode = SelectedChannel.Code,
                        Category = "Teste",
                        Origin = "Teste Canal",
                        From = new MessageFromDto
                        {
                            Name = AdminService.LoggedUser.User.Name,
                            Email = AdminService.LoggedUser.User.MainEmail,
                            Phone = AdminService.LoggedUser.User.PhoneNumber
                        },
                        Subject = "Teste",
                        Body = $"Mensagem de teste = {DateTime.Now}"
                    };
                    var result = await AdminService.MessagesService.SendMessageAsync(message);
                    if (result)
                    {
                        Message = "Mensagem de teste enviada com sucesso";
                        ShowMessage = true;
                    }
                    else
                    {
                        Message = "Mensagem de teste não enviada";
                        ShowMessage = true;
                    }
                    StateHasChanged();
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messageChannels");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task LoadGroups()
        {
            try
            {
                AllUsers = await AdminService.UsersService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                AllGroups = await AdminService.UserGroupsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                Channels = await AdminService.MessageChannelsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messageChannels");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}