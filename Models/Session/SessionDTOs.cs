
namespace nauth_asp.Models
{
    public class SessionBasicDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ServiceId { get; set; }
        public bool Is2FAConfirmed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Os { get; set; } = string.Empty;
    }

    public class SessionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ServiceId { get; set; }
        public bool Is2FAConfirmed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Os { get; set; } = string.Empty;
        public ServiceBasicDTO? Service { get; set; }
    }

    public class FullSessionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ServiceId { get; set; }
        public bool Is2FAConfirmed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Os { get; set; } = string.Empty;
        public ServiceDTO? Service { get; set; }
        public UserDTO? User { get; set; }
    }

}
