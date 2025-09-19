using System.Security.Claims;

namespace nauth_asp.Helpers
{
    public class FClaim
    {
        public static Claim? FindClaim(ClaimsPrincipal? Principal, string Type)
        {
            return Principal?.Claims?.FirstOrDefault(C => C.Type.Contains(Type, StringComparison.OrdinalIgnoreCase));
        }
    }

}