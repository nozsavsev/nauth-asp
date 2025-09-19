using Microsoft.AspNetCore.Authorization;
using nauth_asp.Repositories;
using nauth_asp.Services;

namespace nauth_asp.AuthHandlers.AuthorizationRequirments
{
    public sealed class UserOwnsServiceRequirement : IAuthorizationRequirement { }

    public sealed class UserOwnsServiceHandler(ServiceRepository serviceRepository, IHttpContextAccessor HttpContextAccessor) : AuthorizationHandler<UserOwnsServiceRequirement, long>
    {
        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          UserOwnsServiceRequirement requirement,
          long serviceId)
        {
            var sub = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (sub is null) return;

            var uid = long.Parse(sub);
            var isOwner = (await serviceRepository.DynamicQuerySingleAsync(q => q.Where(s => s.Id == serviceId && s.userId == uid))) != null;

            if (isOwner) context.Succeed(requirement);
            else
            {
                if (context.User.HasClaim(c => c.Type == NauthPermissions.PrManageServices.ToString() && c.Value == "true"))
                {
                    context.Succeed(requirement);
                    return;
                }

                HttpContextAccessor.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.ForeginResource);
                context.Fail();
            }
        }
    }
}
