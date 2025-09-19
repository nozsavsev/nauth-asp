using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.AuthHandlers
{
    public class SessionValidator(SessionService sessionService, UserService userService, ServiceService serviceService)
    {
        public async Task<SessionValidationResult> ValidateAsync(long userId, long sessionId, long? serviceId = null)
        {

            var session = await sessionService.GetByIdAsync(sessionId);

            if (session == null)
                return new SessionValidationResult
                {
                    IsActive = false,
                    Reason = AuthFailureReasons.SessionExpired
                };

            if (session.userId != userId && serviceId == null)
                return new SessionValidationResult
                {
                    IsActive = false,
                    Reason = AuthFailureReasons.SessionExpired
                };

            var user = await userService.GetByIdAsync(session.userId);

            if (user == null)
                return new SessionValidationResult
                {
                    IsActive = false,
                    Reason = AuthFailureReasons.SessionExpired
                };

            if (user.isEnabled == false)
                return new SessionValidationResult
                {
                    IsActive = false,
                    Reason = AuthFailureReasons.RequireEnabledUser
                };

            DB_Service? service = null;
            if (serviceId != null)
            {
                service = await serviceService.GetByIdAsync(serviceId.Value);
                if (service == null)
                    return new SessionValidationResult
                    {
                        IsActive = false,
                        Reason = AuthFailureReasons.SessionExpired
                    };
            }

            return new SessionValidationResult
            {
                IsActive = session.ExpiresAt > DateTime.UtcNow && ((serviceId == null && session.userId == userId)  || session.serviceId == serviceId),
                Session = session,
                User = user,
                Service = service
            };
        }
    }

    public sealed class SessionValidationResult
    {
        public bool IsActive { get; init; }
        public DB_Session? Session { get; init; }
        public DB_User? User { get; init; }
        public DB_Service? Service { get; init; }
        public AuthFailureReasons Reason { get; init; }
    }
}
