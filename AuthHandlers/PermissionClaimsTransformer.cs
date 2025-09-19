using Microsoft.AspNetCore.Authentication;
using nauth_asp.Services;
using System.Security.Claims;

namespace nauth_asp.AuthHandlers
{
    public sealed class PermissionClaimsTransformer(PermissionService perms, IHttpContextAccessor http) : IClaimsTransformation
    {

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not { IsAuthenticated: true }) return principal;

            if (principal.HasClaim("enriched", "1")) return principal;

            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var sid = principal.FindFirst("sid")?.Value;
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(sid)) return principal;

            var permissions = await perms.GetUserPermissionsAsync(long.Parse(sub));

            var id = new ClaimsIdentity("app");
            foreach (var perm in permissions)
                id.AddClaim(new Claim(perm.key, "true"));

            id.AddClaim(new Claim("enriched", "1"));

            var clone = new ClaimsPrincipal(principal.Identities);
            clone.AddIdentity(id);
            return clone;
        }
    }
}
