using nauth_asp.Helpers;
using nauth_asp.Models.EmailAction;
using nauth_asp.Models.Permissions;
using nauth_asp.Models.Service;
using nauth_asp.Models.Session;
using nauth_asp.Models.TwoFactorAuth;

namespace nauth_asp.Models
{
    public class DB_User
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string email { get; set; } = string.Empty;
        public bool isEmailVerified { get; set; } = false;

        public string passwordHash { get; set; } = string.Empty;
        public string passwordSalt { get; set; } = string.Empty;

        public virtual List<DB_Session> sessions { get; set; } = new();
        public virtual List<DB_EmailAction> emailActions { get; set; } = new();
        public virtual List<DB_UserPermission> permissions { get; set; } = new();
        public virtual List<DB_UserService> services { get; set; } = new();
        public virtual List<DB_2FAEntry> _2FAEntries { get; set; } = new();
        public virtual List<DB_Service> ownedServices { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
