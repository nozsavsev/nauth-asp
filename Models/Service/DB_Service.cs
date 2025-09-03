using nauth_asp.Helpers;
using nauth_asp.Models.Permissions;
using nauth_asp.Models.Session;

namespace nauth_asp.Models.Service
{
    public class DB_Service
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string name { get; set; } = string.Empty;

        public long userId { get; set; } = -1;
        public virtual DB_User? user { get; set; } = null;

        public virtual List<DB_Session>? sessions { get; set; } = new();
        public virtual List<DB_Permission>? permissions { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
