using nauth_asp.Helpers;
using nauth_asp.Models.Service;

namespace nauth_asp.Models.Permissions
{
    public class DB_Permission
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string name { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;

        public long ServiceId { get; set; } = 0;
        public virtual DB_Service? Service { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
