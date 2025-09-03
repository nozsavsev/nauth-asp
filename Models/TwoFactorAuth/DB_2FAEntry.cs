using nauth_asp.Helpers;

namespace nauth_asp.Models.TwoFactorAuth
{
    public class DB_2FAEntry
    {
        public long Id { get; set; } = SnowflakeGlobal.Generate();

        public string name { get; set; } = string.Empty;
        public string recoveryCode { get; set; } = string.Empty;
        public string _2faSecret { get; set; } = string.Empty;

        public long userId { get; set; } = -1;
        public virtual DB_User? user { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
