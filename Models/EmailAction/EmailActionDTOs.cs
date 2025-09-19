namespace nauth_asp.Models
{
    public class EmailActionDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Data { get; set; }
        public EmailActionType Type { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }


        public class DecodedEmailActionDTO
    {
        public string Id { get; set; }
        public UserBasicDTO User { get; set; }
        public string Data { get; set; }
        public EmailActionType Type { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
