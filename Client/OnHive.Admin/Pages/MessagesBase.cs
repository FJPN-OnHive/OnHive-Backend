using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Messages;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using System.Security.Principal;

namespace OnHive.Admin.Pages
{
    public class MessagesBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        public List<MessageUserDto> UserMessages { get; set; } = [];

        public MessageUserDto? SelectedMessage { get; set; } = new();

        public MessageDto? SelectedFullMessage { get; set; } = null;

        public string Message { get; set; } = string.Empty;

        public bool ShowMessage { get; set; } = false;

        public bool ShowDeleteMessage { get; set; } = false;

        public string SearchMessage { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public bool LoadingFullMail { get; set; } = false;

        public bool NewOnly { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/messages");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/messages");
            }
            await LoadMessages();
        }

        public async Task MarkRead()
        {
            if (SelectedMessage != null)
            {
                SelectedMessage.Status = Core.Library.Enums.Messages.MessageStatus.Read;
                SelectedMessage.ReadDate = DateTime.UtcNow;
                await SaveMessage(false);
            }
        }

        public async Task MarkUnread()
        {
            if (SelectedMessage != null)
            {
                SelectedMessage.Status = Core.Library.Enums.Messages.MessageStatus.New;
                await SaveMessage(false);
            }
        }

        public async Task DeleteMessage()
        {
            if (SelectedMessage != null)
            {
                SelectedMessage.Status = Core.Library.Enums.Messages.MessageStatus.Deleted;
                await SaveMessage(true);
            }
        }

        public async Task SaveMessage(bool showMessage)
        {
            try
            {
                Message = string.Empty;
                if (SelectedMessage != null)
                {
                    SelectedMessage.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedMessage = await AdminService.MessagesService.SaveUserMessageAsync(SelectedMessage, AdminService.LoggedUser.Token);
                    if (showMessage)
                    {
                        Message = "Mensagem salva com sucesso";
                        ShowMessage = true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void Clear()
        {
            SelectedMessage = null;
            SelectedFullMessage = null;
            StateHasChanged();
        }

        public string RowClassSelector(MessageUserDto item, int rowIndex)
        {
            return (item.Equals(SelectedMessage))
                ? "mud-info"
                : "";
        }

        public async Task SelectMessage(MessageUserDto messageUserDto)
        {
            try
            {
                LoadingFullMail = true;
                SelectedMessage = messageUserDto;
                SelectedFullMessage = await AdminService.MessagesService.GetById(SelectedMessage.MessageId, AdminService?.LoggedUser?.Token ?? string.Empty);
                await MarkRead();
                LoadingFullMail = false;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadMessages()
        {
            try
            {
                Loading = true;
                UserMessages = await AdminService.MessagesService.GetAllByUser(NewOnly, AdminService?.LoggedUser?.Token ?? string.Empty);
                Loading = false;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}