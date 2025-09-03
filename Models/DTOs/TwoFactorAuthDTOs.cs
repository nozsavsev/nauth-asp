namespace nauth_asp.Models.DTOs
{
    public class TwoFactorAuthDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TwoFactorAuthSetupDTO
    {
        public string TwoFactorSecret { get; set; } = string.Empty;
        public string RecoveryCode { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
