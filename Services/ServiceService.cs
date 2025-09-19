using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace nauth_asp.Services
{
    public class ServiceService(
        ServiceRepository repository,
    IConfiguration config,
    SessionRepository sessionRepository,
    UserService userService
    ) : GenericService<DB_Service>(repository)
    {

        public async Task EnsureMother()
        {

            if (await GetByIdAsync(0) == null)
            {
                var mother = new DB_Service
                {
                    Id = 0,
                    name = "NAUTH",
                };

                await repository.AddAsync(mother);

            }
        }

        public async Task<DB_Service> CreateService(string name, long userId)
        {
            if (name.Length > 255)
                name = name.Substring(0, 255);
            var service = new DB_Service
            {
                name = name,
                userId = userId
            };

            return await repository.AddAsync(service);
        }

        public async Task DeleteServiceByIdAsync(long id)
        {
            await repository.DeleteByIdAsync(id);
        }

        public async Task<DB_Service> UpdateService(long serviceId, string name)
        {
            var service = await repository.GetByIdAsync(serviceId, tracking: true);
            if (service == null)
                throw new NauthException(WrResponseStatus.NotFound);
            if (name.Length > 255)
                name = name.Substring(0, 255);
            service.name = name;
            return await repository.UpdateAsync(service);
        }

        public async Task RevokeServiceSession(long sessionId)
        {
            await sessionRepository.DeleteByIdAsync(sessionId);
        }

        public async Task<string?> CreateServiceSession(long serviceId, DateTime? expiresAt = null)
        {

            var service = await repository.GetByIdAsync(serviceId);

            if (service == null)
                throw new NauthException(WrResponseStatus.NotFound, "Service not found");

            var session = new DB_Session
            {
                serviceId = serviceId,
                userId = service!.userId! ?? 0,
                jwtHash = string.Empty,
                is2FAConfirmed = false,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!))
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:secretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("serviceId", serviceId.ToString()),
                new Claim("sid", session.Id.ToString())
            };

            var _token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"]!,
                audience: config["JWT:Audience"]!,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(_token);

            session.jwtHash = SHA256.Compute(token);

            var result = await sessionRepository.AddAsync(session);

            return result != null ? token : null;
        }

        public async Task<List<DB_Service>> GetAllAsync()
        {
            return await repository.DynamicQueryManyAsync(q => q.IgnoreAutoIncludes()
                                                                .Include(s => s.permissions)
                                                                .Include(s => s.sessions)
                                                                .Include(s => s.user));
        }

        public async Task<List<DB_Service>> GetAllOwnedAsync(long userId)
        {
            return await repository.DynamicQueryManyAsync(q => q.Where(s => s.userId == userId).IgnoreAutoIncludes()
                                                                       .Include(s => s.permissions)
                                                                       .Include(s => s.sessions)
                                                                       .Include(s => s.user));
        }

        public async Task<DB_Session> DecodeAndVerifyUserAuthToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(config["JWT:secretKey"]!);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JWT:Issuer"],
                    ValidAudience = config["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken)
                {
                    throw new NauthException(WrResponseStatus.Unauthorized, "Invalid token type");
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionIdClaim = principal.FindFirst("sid")?.Value;

                if (!long.TryParse(userIdClaim, out var userId) || !long.TryParse(sessionIdClaim, out var sessionId))
                {
                    throw new NauthException(WrResponseStatus.Unauthorized, "Invalid claims");
                }

                var session = await sessionRepository.DynamicQuerySingleAsync(
                    q => q.Include(s => s.user)
                    .Include(s => s.user.sessions)
                    .Include(s => s.user.emailActions)
                    .Include(s => s.user.permissions)
                    .Include(s => s.user.Services)
                    .Include(s => s.user._2FAEntries)
                    
                    
                    , false, true);

                if (session == null)
                {
                    throw new NauthException(WrResponseStatus.NotFound, "Session not found");
                }

                if (session.userId != userId)
                {
                    throw new NauthException(WrResponseStatus.Unauthorized, "User ID mismatch");
                }

                if (session.ExpiresAt < DateTime.UtcNow)
                {
                    throw new NauthException(WrResponseStatus.Unauthorized, "Session expired");
                }

                if (session.jwtHash != SHA256.Compute(token))
                {
                    throw new NauthException(WrResponseStatus.Unauthorized, "Invalid token hash");
                }

                if (session.user == null)
                {
                    throw new NauthException(WrResponseStatus.NotFound, "User not found for session");
                }

                return session;
            }
            catch (SecurityTokenException e)
            {
                throw new NauthException(WrResponseStatus.Unauthorized, "Token validation failed", e);
            }
        }

        public async Task<DB_Service?> GetByIdLoadedAsync(long id)
        {
            return await _repository.DynamicQuerySingleAsync(q => q.Where(s => s.Id == id).Include(s => s.user).Include(s => s.permissions).Include(s => s.sessions), loadDependencies: false);
        }
    }
}