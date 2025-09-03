using System.ComponentModel.DataAnnotations;

namespace nauth_asp.Models.DTOs
{
    public class UserDTO
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SessionDTO> Sessions { get; set; } = new();
        public List<UserPermissionDTO> Permissions { get; set; } = new();
        public List<UserServiceDTO> Services { get; set; } = new();
        public List<TwoFactorAuthDTO> TwoFactorAuthEntries { get; set; } = new();
    }

    public class UserBasicDTO
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserDTO
    {
        [MinLength(1)]
        [MaxLength(240)]
        public string? Name { get; set; }
    }

    public class UserLoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
