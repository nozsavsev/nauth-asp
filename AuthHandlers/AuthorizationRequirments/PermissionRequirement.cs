using Microsoft.AspNetCore.Authorization;

namespace nauth_asp.AuthHandlers.AuthorizationRequirments
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == requirement.Permission && c.Value == "true"))
            {
                context.Succeed(requirement);
            }
            else
            {
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.AddAuthenticationFailureReason(Enum.Parse<AuthFailureReasons>(requirement.Permission));
                }
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
