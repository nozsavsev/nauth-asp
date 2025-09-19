using nauth_asp.Models;

namespace nauth_asp
{
    public static class HttpContextExtensions
    {
        public static DB_User? NauthUser(this HttpContext context)
        {
            return context.Items?["User"] as DB_User;

        }

        public static DB_Session? NauthSession(this HttpContext context)
        {
            return context.Items?["Session"] as DB_Session;
        }

        public static DB_Service? NauthService(this HttpContext context)
        {
            return context.Items?["Service"] as DB_Service;
        }

        public static void AddAuthenticationFailureReason(this HttpContext context, AuthFailureReasons reason)
        {

            var reasonStr = reason.ToString();

            if (context.Items["AuthenticationFailureReason"] == null)
                context.Items["AuthenticationFailureReason"] = new List<string>() { reasonStr };
            else
                (context.Items["AuthenticationFailureReason"] as List<string>)?.Add(reasonStr);
        }

        public static List<AuthFailureReasons>? GetAuthenticationFailureReasons(this HttpContext context)
        {
            return (context.Items["AuthenticationFailureReason"] as List<string>)?.Select(r => Enum.Parse<AuthFailureReasons>(r)).ToList() ?? new();
        }

        public static string GetRealIpAddress(this HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? string.Empty;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
