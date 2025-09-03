using nauth_asp.Helpers;

namespace nauth_asp.Models.Permissions
{
    public class DB_UserPermission
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public long permissionId { get; set; } = -1;
        public virtual DB_Permission? permission { get; set; } = null;

        public long userId { get; set; } = -1;
        public virtual DB_User? user { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
