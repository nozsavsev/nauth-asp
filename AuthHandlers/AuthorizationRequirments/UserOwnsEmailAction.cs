using Microsoft.AspNetCore.Authorization;
using nauth_asp.Repositories;

namespace nauth_asp.AuthHandlers.AuthorizationRequirments
{
    public sealed class UserOwnsEmailActionRequirement : IAuthorizationRequirement { }

    public sealed class UserOwnsEmailActionHandler(EmailActionRepository emailActionRepository, IHttpContextAccessor HttpContextAccessor) : AuthorizationHandler<UserOwnsEmailActionRequirement, long>
    {
        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          UserOwnsEmailActionRequirement requirement,
          long emailActionId)
        {
            var sub = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (sub is null) return;

            var uid = long.Parse(sub);
            var isOwner = (await emailActionRepository.DynamicQuerySingleAsync(q => q.Where(s => s.Id == emailActionId && s.userId == uid))) != null;

            if (isOwner) context.Succeed(requirement);
            else
            {
                HttpContextAccessor.HttpContext.AddAuthenticationFailureReason(AuthFailureReasons.ForeginResource);
                context.Fail();
            }
        }
    }
}
