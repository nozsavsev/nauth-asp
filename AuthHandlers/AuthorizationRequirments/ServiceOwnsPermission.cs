using Microsoft.AspNetCore.Authorization;
using nauth_asp.Repositories;

namespace nauth_asp.AuthHandlers.AuthorizationRequirments
{
    public sealed class ServiceOwnsPermissionRequirement : IAuthorizationRequirement { }

    public sealed class ServiceOwnsPermissionHandler(PermissionRepository permissionRepository, IHttpContextAccessor HttpContextAccessor) : AuthorizationHandler<ServiceOwnsPermissionRequirement, long>
    {
        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          ServiceOwnsPermissionRequirement requirement,
          long permissionId)
        {
            var sub = context.User.FindFirst("serviceId")?.Value;
            if (sub is null) return;

            var serviceId = long.Parse(sub);
            var isOwner = (await permissionRepository.DynamicQuerySingleAsync(p => p.Where(s => s.ServiceId == serviceId && s.Id == permissionId))) != null;

            if (isOwner) context.Succeed(requirement);
            else
            {
                HttpContextAccessor.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.ForeginResource);
                context.Fail();
            }
        }
    }
}
