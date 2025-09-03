namespace nauth_asp.Models.DTOs
{
    public class PermissionDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public long ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceBasicDTO? Service { get; set; }
    }

    public class PermissionBasicDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePermissionDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public long ServiceId { get; set; }
    }

    public class UpdatePermissionDTO
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
    }
}
