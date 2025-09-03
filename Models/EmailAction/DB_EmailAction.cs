using nauth_asp.Helpers;
using nauth_asp.Models.Service;

namespace nauth_asp.Models.EmailAction
{
    public enum EmailActionType
    {
        VerifyEmail,
        PasswordReset,
        EmailCode, //used as 2fa when adding 2fa methods or as an email code login method
        DeleteAccount, 
        ChangeEmail,
    }

    public class DB_EmailAction
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string token { get; set; } = string.Empty;

        public string? data { get; set; } = null;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}