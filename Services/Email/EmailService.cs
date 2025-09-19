using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace nauth_asp.Services
{
    public class EmailService(IConfiguration config, ILogger<EmailService> log) : IEmailService
    {


        private readonly string _sender = config["Amazon:SES_Sender"]!;

        private readonly string _access = config["Amazon:ACCESS_KEY"]!;
        private readonly string _secret = config["Amazon:SECRET_KEY"]!;
        private readonly string _region = config["Amazon:SES_REGION"]!;

        private AmazonSimpleEmailServiceClient _sesClient
        {
            get
            {
                var credentials = new BasicAWSCredentials(_access, _secret);
                var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region);
                return new AmazonSimpleEmailServiceClient(credentials, regionEndpoint);
            }
        }

        public async Task<bool> SendEmailAsync(string destination, string subject, string body, string htmlBody)
        {

            try
            {
                var _destination = new Destination
                {
                    ToAddresses = new List<string> { destination }
                };

                var subjectContent = new Content(subject);
                var _textBody = new Content(body);
                var _htmlBody = new Content(htmlBody);

                var _body = new Body
                {
                    Text = _textBody,
                    Html = _htmlBody
                };

                var message = new Message
                {
                    Subject = subjectContent,
                    Body = _body
                };

                var sendEmailRequest = new SendEmailRequest
                {
                    Source = _sender,
                    Destination = _destination,
                    Message = message
                };

                var response = await _sesClient.SendEmailAsync(sendEmailRequest);


                log.LogInformation("Sent email to {destination} with status code {statusCode}", destination, response.HttpStatusCode);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                log.LogError($"Error sending email: {ex.Message}");
                return false;
            }

        }
    }
}
