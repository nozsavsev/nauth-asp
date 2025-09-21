using Microsoft.AspNetCore.Authorization;

namespace nauth_asp.AuthHandlers
{
    public class ValidSessionRequirement : IAuthorizationRequirement
    {

        public ValidSessionRequirement(

               bool requireVerifiedEmail = true,
               bool requireEnabledUser = true,

               bool ignore2FA = false,
               bool require2FASetup = false,

               bool isServiceSession = false,

               bool allowServices = false
               )
        {
            RequireVerifiedEmail = requireVerifiedEmail;
            RequireEnabledUser = requireEnabledUser;
            Ignore2FA = ignore2FA;
            Require2FASetup = require2FASetup;
            IsServiceSession = isServiceSession;
            AllowServices = allowServices;

            if (IsServiceSession)
            {
                Require2FASetup = true;
                Ignore2FA = false;
                RequireEnabledUser = true;
                RequireVerifiedEmail = true;
                AllowServices = true;
            }

        }

        public bool RequireVerifiedEmail;
        public bool RequireEnabledUser;
        public bool Ignore2FA;
        public bool Require2FASetup;
        public bool IsServiceSession;
        public bool AllowServices;
    }


    public class ValidSessionHandler : AuthorizationHandler<ValidSessionRequirement>
    {
        private IHttpContextAccessor httpContextAccessor;


        public ValidSessionHandler(IHttpContextAccessor HttpContextAccessor)
        {
            httpContextAccessor = HttpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidSessionRequirement requirement)
        {

            HttpContext? httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                context.Fail(new AuthorizationFailureReason(this, "HttpContextNull"));

                return Task.CompletedTask;
            }

            if(requirement.AllowServices == false && httpContext?.NauthSession()?.serviceId != null)
            {
                httpContext?.AddAuthenticationFailureReason(AuthFailureReasons.ForeginResource);
                context.Fail(new AuthorizationFailureReason(this, "ServiceSessionNotAllowed"));
                return Task.CompletedTask;
            }

            if (requirement.Ignore2FA)
            {
                Console.WriteLine("ignoring 2FA");
                ;//ignore All 2FA requirments
            }
            else
            {
                if (requirement.Require2FASetup) //2fa needs to be set up
                {
                    Console.WriteLine("2FA setup enforcment");

                    if ((httpContext?.NauthUser()?._2FAEntries?.Where(fa => fa.isActive)?.Count() ?? 0) == 0)
                    {
                        Console.WriteLine("2FA setup enforcment pushed");


                        context.Fail(new AuthorizationFailureReason(this, "Require2FASetup"));
                        httpContext?.AddAuthenticationFailureReason(AuthFailureReasons.Require2FASetup);
                    }

                }

                if (requirement.Ignore2FA == false && (httpContext?.NauthUser()?._2FAEntries?.Where(fa => fa.isActive)?.Count() ?? 0) > 0)
                {
                    Console.WriteLine("2FA enforcment");


                    if (httpContext?.NauthSession() == null || httpContext?.NauthSession()?.is2FAConfirmed == false)
                    {
                        Console.WriteLine("2FA pushed");


                        context.Fail(new AuthorizationFailureReason(this, "_2FARequired"));
                        httpContext?.AddAuthenticationFailureReason(AuthFailureReasons._2FARequired);
                    }

                }

            }


            if (requirement.RequireVerifiedEmail)
            {

                //var ev = httpContext?.PharmaUser()?.EmailVerified;
                //var evf = httpContext?.PharmaUser()?.EmailVerified ?? false;

                if ((httpContext?.NauthUser()?.isEmailVerified ?? false) == false)
                {
                    context.Fail(new AuthorizationFailureReason(this, "RequireVerifiedEmail"));
                    httpContext?.AddAuthenticationFailureReason(AuthFailureReasons.RequireVerifiedEmail);
                }
            }

            if (requirement.RequireEnabledUser)
            {
                var user = httpContext?.NauthUser();

                if ((httpContext?.NauthUser()?.isEnabled ?? false) == false)
                {
                    context.Fail(new AuthorizationFailureReason(this, "RequireEnabledUser"));
                    httpContext?.AddAuthenticationFailureReason(AuthFailureReasons.RequireEnabledUser);
                }
            }

            if (!context.HasFailed)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }

}
