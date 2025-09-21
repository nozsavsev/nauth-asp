using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using nauth_asp.SignalRHubs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UAParser;

namespace nauth_asp.Services
{
    public class SessionService(SessionRepository sessionRepository, IConfiguration config, IHttpContextAccessor httpContextAccessor, IHubContext<AuthHub> hubContext, IUserRefreshService userRefreshService) : GenericService<DB_Session>(sessionRepository)
    {

        public async Task<string?> IssueSession(long userId, long? serviceId = null, DateTime? expiresAt = null)
        {
            var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
            var c = Parser.GetDefault().Parse(userAgent);

            var session = new DB_Session
            {
                userId = userId,
                serviceId = serviceId,
                jwtHash = string.Empty,
                is2FAConfirmed = false,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!)),
                IpAddress = httpContextAccessor.HttpContext?.GetRealIpAddress() ?? "unknown",
                Device = c.Device.Family,
                Browser = c.UA.Family,
                Os = c.OS.Family,
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:secretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("sid", session.Id.ToString())
            };

            var _token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"]!,
                audience: config["JWT:Audience"]!,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: session.ExpiresAt,
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(_token);

            session.jwtHash = SHA256.Compute(token);

            var result = await sessionRepository.AddAsync(session);

            return result != null ? token : null;
        }

        public async Task<List<DB_Session>> GetByUserIdAsync(long userId)
        {
            var sessions = await sessionRepository.GetByUserIdAsync(userId);
            return sessions;
        }

        public async Task<List<DB_Session>> GetByServiceIdAsync(long serviceId)
        {
            var sessions = await sessionRepository.GetByServiceIdAsync(serviceId);
            return sessions;
        }

        public async Task<List<DB_Session>> GetAllAsync()
        {
            var sessions = await sessionRepository.DynamicQueryManyAsync();
            return sessions;
        }

        public async Task<List<DB_Session>> GetActiveSessionsAsync()
        {
            var sessions = await sessionRepository.GetActiveSessionsAsync();
            return sessions;
        }

        public async Task<List<DB_Session>> GetExpiredSessionsAsync()
        {
            var sessions = await sessionRepository.GetExpiredSessionsAsync();
            return sessions;
        }

        public async Task DeleteAsync(long id)
        {
            var session = await sessionRepository.GetByIdAsync(id);
            if (session != null)
            {
                await sessionRepository.DeleteAsync(session);
            }
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await sessionRepository.ExistsAsync(id);
        }

        public async Task<int> CleanupExpiredSessionsAsync()
        {
            return await sessionRepository.DeleteExpiredSessionsAsync();
        }

        internal async Task RevokeSessionAsync(long Id)
        {
            var session = await sessionRepository.GetByIdAsync(Id);
            if (session == null) return;
            await hubContext.Clients.Group(Id.ToString()).SendAsync("Logout");
            userRefreshService.QueueUserRefresh(session.userId);
            await sessionRepository.DeleteByIdAsync(Id);
        }

        internal async Task RevokeAllSessions(long userId, long? ignoreSessionId = null)
        {
            var sessions = await sessionRepository.DynamicQueryManyAsync(q => q.Where(s => s.userId == userId));

            foreach (var session in sessions)
            {
                if (session.Id != ignoreSessionId && session.serviceId == null)
                {
                    await RevokeSessionAsync(session.Id);
                }
            }
        }
    }
}
