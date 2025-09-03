namespace nauth_asp.Models.DTOs
{
    public class ServiceDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SessionDTO> Sessions { get; set; } = new();
        public List<PermissionDTO> Permissions { get; set; } = new();
    }

    public class ServiceBasicDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceDTO
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateServiceDTO
    {
        public string? Name { get; set; }
        public List<PermissionBasicDTO> Permissions { get; set; } = new();
    }
}
