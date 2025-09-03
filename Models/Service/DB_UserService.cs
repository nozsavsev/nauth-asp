using nauth_asp.Helpers;
using nauth_asp.Models.Session;

namespace nauth_asp.Models.Service
{
    public class DB_UserService
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();
 
        public long userId { get; set; } = -1;
        public virtual DB_User? user { get; set; } = null;

        public long serviceId { get; set; } = -1;
        public virtual DB_Service? service { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
