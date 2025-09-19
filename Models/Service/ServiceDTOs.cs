
namespace nauth_asp.Models
{

    public class ServiceDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } 
        public UserBasicDTO user { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SessionBasicDTO> Sessions { get; set; } = new();
        public List<PermissionBasicDTO> Permissions { get; set; } = new();//permissions that are managed by this service
    }

    public class ServiceBasicDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } //can't be user object as will cause circular dependency
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceDTO
    {
        public string Name { get; set; } = string.Empty;
    }
}
