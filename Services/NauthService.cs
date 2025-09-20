using Microsoft.AspNetCore.Http.HttpResults;
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
    public class NauthService(

    IConfiguration config,
    SessionRepository sessionRepository,
    UserService userService,
    ServiceService serviceService,
    PermissionService permissionService,
    UserPermissionService userPermissionService
    )
    {


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

                if (session.is2FAConfirmed == false && session.user._2FAEntries.Where(e => e.isActive).Count() > 0)
                {
                    throw new NauthException(WrResponseStatus._2FARequired, "User not found for session");
                }

                if (session.user.isEnabled == false)
                {
                    throw new NauthException(WrResponseStatus.RequireEnabledUser, "User not found for session");
                }

                return session;
            }
            catch (SecurityTokenException e)
            {
                throw new NauthException(WrResponseStatus.Unauthorized, "Token validation failed", e);
            }
        }

        public async Task<DB_Permission> CreatePermission(CreatePermissionDTO permission)
        {
            return await permissionService.CreatePermission(permission);
        }

        public async Task DeletePermission(long permission)
        {
            await permissionService.DeleteByidAsync(permission);
        }

        internal async Task UpdateUserPermissions(ServiceUpdateUserPermissionsDTO updateSet)
        {

            var user = await userService.GetByIdAsync(long.Parse(updateSet.UserId));

            if(user == null)
                throw new NauthException(WrResponseStatus.NotFound, "User not found");

            var permissionsList = user.permissions.Select(up => up.permissionId).ToList();

            var toRemove = updateSet.permissions.Where(a => a.Action == ServiceUpdateUserPermissionsDTO.ServicePermissionOnUserUpdateDTOInner.RequestAction.Remove).Select(p => long.Parse(p.PermissionId!));
            var tAdd = updateSet.permissions.Where(a => a.Action == ServiceUpdateUserPermissionsDTO.ServicePermissionOnUserUpdateDTOInner.RequestAction.Add).Select(p => long.Parse(p.PermissionId!));

            permissionsList.AddRange(tAdd);
            permissionsList = permissionsList.Where(p => !toRemove.Contains(p)).ToList();

            await userService.UpdatePermissions(user.Id, permissionsList);
        }
    }
}