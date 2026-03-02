namespace EHive.Core.Library.Entities.Users
{
    public class User : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string MainEmail => Emails.FirstOrDefault(e => e.IsMain)?.Email ?? string.Empty;

        public List<UserEmail> Emails { get; set; } = new List<UserEmail>();

        public string PasswordHash { get; set; } = string.Empty;

        public string SocialLoginToken { get; set; } = string.Empty;

        public List<ValidationCode> ChangePasswordCodes { get; set; } = new();

        public bool IsChangePasswordRequested { get; set; }

        public string SocialName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Nationality { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string MobilePhoneNumber { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public List<UserDocument> Documents { get; set; } = new();

        public ValidationCode RedirectLoginCode { get; set; } = new();

        public bool IsForeigner { get; set; } = false;

        public string Occupation { get; set; } = string.Empty;

        public List<Address> Addresses { get; set; } = [];

        public TempPassword? TempPassword { get; set; }
    }

    public class UserDocument
    {
        public string DocumentNumber { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;
    }

    public class UserEmail
    {
        public bool IsMain { get; set; } = false;

        public string Email { get; set; } = string.Empty;

        public bool IsValidated { get; set; } = false;

        public List<ValidationCode> ValidationCodes { get; set; } = new();
    }

    public class ValidationCode
    {
        public DateTime ExpirationDate { get; set; }

        public string Code { get; set; } = string.Empty;

        public bool Remember { get; set; } = false;
    }

    public class TempPassword
    {
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }
}