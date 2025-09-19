using nauth_asp.Helpers;

namespace nauth_asp.Models
{
    public class DB_Session
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string jwtHash { get; set; } = string.Empty;

        public long userId { get; set; } = -1;
        public virtual DB_User? user { get; set; } = null;

        public long? serviceId { get; set; } = null;
        public virtual DB_Service? service { get; set; } = null;

        public bool is2FAConfirmed { get; set; } = false;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string IpAddress { get; set; } = string.Empty;

        public string Device { get; set; } = "unknown";

        public string Browser { get; set; } = "unknown";

        public string Os { get; set; } = "unknown";
    }
}
