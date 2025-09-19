using nauth_asp.Helpers;

namespace nauth_asp.Models
{
    public class DB_Permission
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string name { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;

        public long ServiceId { get; set; } = 0;
        public virtual DB_Service? Service { get; set; } = null;

        public virtual List<DB_UserPermission> UserPermissions { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
