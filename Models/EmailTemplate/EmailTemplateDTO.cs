using nauth_asp.Helpers;

namespace nauth_asp.Models
{

    public class EmailTemplateDTO
    {
        public string Id { get; set; } = SnowflakeGlobal.Generate().ToString();

        public string Name { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;

        public EmailTemplateType Type { get; set; } = EmailTemplateType.VerifyEmail;

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string htmlBody { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public class CreateEmailTemplateDTO
    {
        public string Name { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;

        public EmailTemplateType Type { get; set; } = EmailTemplateType.VerifyEmail;

        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string htmlBody { get; set; } = string.Empty;

    }

}
