using nauth_asp.Helpers;
using nauth_asp.Models.Service;

namespace nauth_asp.Models.Session
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

    }
}
