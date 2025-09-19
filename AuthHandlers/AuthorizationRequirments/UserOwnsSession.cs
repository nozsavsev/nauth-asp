using Microsoft.AspNetCore.Authorization;
using nauth_asp.Repositories;

namespace nauth_asp.AuthHandlers.AuthorizationRequirments
{
    public sealed class UserOwnsSessionRequirement : IAuthorizationRequirement { }

    public sealed class UserOwnsSessionHandler(SessionRepository sessionRepository, IHttpContextAccessor HttpContextAccessor) : AuthorizationHandler<UserOwnsSessionRequirement, long>
    {
        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          UserOwnsSessionRequirement requirement,
          long sessionId)
        {
            var sub = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (sub is null) return;

            var uid = long.Parse(sub);
            var isOwner = (await sessionRepository.DynamicQuerySingleAsync(q => q.Where(s => s.Id == sessionId &&( s.userId == uid || s.service.userId == uid)))) != null;

            if (isOwner) context.Succeed(requirement);
            else
            {
                HttpContextAccessor.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.ForeginResource);
                context.Fail();
            }
        }
    }
}
