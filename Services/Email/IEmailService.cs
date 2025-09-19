namespace nauth_asp.Services
{
    public interface IEmailService
    {
        public Task<bool> SendEmailAsync(string to, string subject, string body, string htmlBody);

    }
}
