using System.ComponentModel.DataAnnotations;

namespace nauth_asp.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarURL { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public bool isEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public List<EmailActionDTO> EmailActions { get; set; } = new();
        public List<SessionDTO> Sessions { get; set; } = new();
        public List<UserPermissionDTO> Permissions { get; set; } = new();
        public List<ServiceDTO> Services { get; set; } = new();
        public List<_2FAEntryDTO> TwoFactorAuthEntries { get; set; } = new();
    }

    public class UserBasicDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarURL { get; set; } = string.Empty;
    }

    public class CreateUserDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? Name { get; set; } = string.Empty;
    }

    public class UpdateNameDTO
    {
        public string Name { get; set; } = string.Empty;
    }

    public class AdminUpdateUserNameDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
