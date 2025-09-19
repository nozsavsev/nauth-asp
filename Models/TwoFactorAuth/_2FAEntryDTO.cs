namespace nauth_asp.Models
{
    public class _2FAEntryDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class _2FAEntrySetupDTO
    {
        public string Id { get; set; }
        public string _2FASecret { get; set; } = string.Empty;
        public string RecoveryCode { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
