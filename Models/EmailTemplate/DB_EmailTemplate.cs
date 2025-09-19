using nauth_asp.Helpers;
using System.Text.Json.Serialization;

namespace nauth_asp.Models
{
    public enum EmailTemplateType
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

        //add other after here to keep compatability with EmailActions

    }

    public class DB_EmailTemplate
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string Name { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;

        public EmailTemplateType Type { get; set; } = EmailTemplateType.VerifyEmail;

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
