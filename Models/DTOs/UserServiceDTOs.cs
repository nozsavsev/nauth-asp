namespace nauth_asp.Models.DTOs
{
    public class UserServiceDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ServiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceBasicDTO? Service { get; set; }
    }

    public class CreateUserServiceDTO
    {
        public long ServiceId { get; set; }
    }
}
