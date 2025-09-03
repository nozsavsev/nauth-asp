namespace nauth_asp.Models.DTOs
{
    public class UserPermissionDTO
    {
        public long Id { get; set; }
        public long PermissionId { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PermissionBasicDTO? Permission { get; set; }
    }

    public class CreateUserPermissionDTO
    {
        public long PermissionId { get; set; }
        public long UserId { get; set; }
    }
}
