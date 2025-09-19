using nauth_asp.Helpers;
using System.Text.Json.Serialization;

namespace nauth_asp.Models
{
    public enum EmailActionType
    {
        [JsonPropertyName("VerifyEmail")]
        VerifyEmail = 0,

        [JsonPropertyName("PasswordReset")]
        PasswordReset,

        [JsonPropertyName("EmailCode")]
        EmailCode, //used as 2fa when adding 2fa methods

        [JsonPropertyName("EmailSignIn")]
        EmailSignIn, //used as an email code login method

        [JsonPropertyName("DeleteAccount")]
        DeleteAccount,

        [JsonPropertyName("ChangeEmail")]
        ChangeEmail,
    }

    public class DB_EmailAction
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public long userId { get; set; } = 0;
        public virtual DB_User? User { get; set; } = null;

        public EmailActionType type { get; set; } = EmailActionType.VerifyEmail;

        public string token { get; set; } = string.Empty;

        public string? data { get; set; } = null;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}