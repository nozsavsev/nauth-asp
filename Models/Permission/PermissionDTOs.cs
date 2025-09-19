
namespace nauth_asp.Models
{
    public class PermissionBasicDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PermissionDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceBasicDTO? Service { get; set; }
    }

    public class CreatePermissionDTO
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string ServiceId { get; set; }
    }

    public class ServiceUpdateUserPermissionsDTO
    {
        public class ServicePermissionOnUserUpdateDTOInner
        {
            public enum RequestAction
            {
                Add,
                Remove
            }
            public string? PermissionId { get; set; }
            public RequestAction? Action { get; set; }
        }




        public string UserId { get; set; }

        public List<ServicePermissionOnUserUpdateDTOInner> permissions { get; set; } = new();

    }
}
