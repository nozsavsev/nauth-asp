using nauth_asp.Helpers;

namespace nauth_asp.Models
{
    public class DB_User
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string? Name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool isEmailVerified { get; set; } = false;
        public bool isEnabled { get; set; } = true;

        public string? AvatarURL { get; set; }

        public string passwordHash { get; set; } = string.Empty;

        public virtual List<DB_Session> sessions { get; set; } = new();
        public virtual List<DB_EmailAction> emailActions { get; set; } = new();
        public virtual List<DB_UserPermission> permissions { get; set; } = new();
        public virtual List<DB_Service> Services { get; set; } = new();
        public virtual List<DB_2FAEntry> _2FAEntries { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
