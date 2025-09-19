namespace nauth_asp.Models
{
    public class UserPermissionDTO
    {
        public string Id { get; set; }
        public string PermissionId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PermissionBasicDTO? Permission { get; set; }
    }

    public class CreateUserPermissionDTO
    {
        public string PermissionId { get; set; }
        public string UserId { get; set; }
    }
}
