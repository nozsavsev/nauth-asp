using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;

namespace nauth_asp.Services
{
    public class NauthService(
        IConfiguration config,
        SessionRepository sessionRepository,
        UserService userService,
        PermissionService permissionService
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
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out var validatedToken
                );

                if (validatedToken is not JwtSecurityToken)
                    throw new NauthException(WrResponseStatus.Unauthorized, "Invalid token type");

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionIdClaim = principal.FindFirst("sid")?.Value;

                if (
                    !long.TryParse(userIdClaim, out var userId)
                    || !long.TryParse(sessionIdClaim, out var sessionId)
                )
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                var session = await sessionRepository.DynamicQuerySingleAsync(
                    q =>
                        q.Where(s => s.Id == sessionId && s.userId == userId)
                            .Include(s => s.user)
                            .Include(s => s.user.sessions)
                            .Include(s => s.user.emailActions)
                            .Include(s => s.user.permissions)
                            .Include(s => s.user.Services)
                            .Include(s => s.user._2FAEntries),
                    true,
                    true
                );

                if (session == null)
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                if (session.userId != userId)
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                if (session.ExpiresAt < DateTime.UtcNow)
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                if (session.jwtHash != SHA256.Compute(token))
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                if (session.user == null)
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons.SessionExpired
                    );

                if (
                    session.is2FAConfirmed == false
                    && session.user._2FAEntries.Where(e => e.isActive).Count() > 0
                )
                    throw new NauthException(
                        WrResponseStatus.BadRequest,
                        AuthFailureReasons._2FARequired
                    );

                if (session.user.isEnabled == false)
                    throw new NauthException(WrResponseStatus.RequireEnabledUser);

                return session;
            }
            catch (SecurityTokenException e)
            {
                Console.WriteLine(e);
                throw new NauthException(
                    WrResponseStatus.BadRequest,
                    AuthFailureReasons.SessionExpired
                );
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                throw new NauthException(
                    WrResponseStatus.BadRequest,
                    AuthFailureReasons.SessionExpired
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new NauthException(
                    WrResponseStatus.BadRequest,
                    AuthFailureReasons.SessionExpired
                );
            }
        }

        public async Task<DB_Permission> CreatePermission(CreatePermissionDTO permission)
        {
            try
            {
                return await permissionService.CreatePermission(permission);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task DeletePermission(long permission)
        {
            try
            {
                await permissionService.DeleteByidAsync(permission);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<DB_Session> UpdateUserPermissions(
            ServiceUpdateUserPermissionsDTO updateSet
        )
        {
            try
            {
                var user = await userService.GetByIdAsync(long.Parse(updateSet.UserId));

                if (user == null)
                    throw new NauthException(WrResponseStatus.NotFound, "User not found");

                var permissionsList = user.permissions.Select(up => up.permissionId).ToList();

                var toRemove = updateSet
                    .permissions.Where(a =>
                        a.Action
                        == ServiceUpdateUserPermissionsDTO
                            .ServicePermissionOnUserUpdateDTOInner
                            .RequestAction
                            .Remove
                    )
                    .Select(p => long.Parse(p.PermissionId!));
                var tAdd = updateSet
                    .permissions.Where(a =>
                        a.Action
                        == ServiceUpdateUserPermissionsDTO
                            .ServicePermissionOnUserUpdateDTOInner
                            .RequestAction
                            .Add
                    )
                    .Select(p => long.Parse(p.PermissionId!));

                permissionsList.AddRange(tAdd);
                permissionsList = permissionsList.Where(p => !toRemove.Contains(p)).ToList();

                await userService.UpdatePermissions(user.Id, permissionsList);

                var session = await sessionRepository.DynamicQuerySingleAsync(
                    q =>
                        q.Where(s =>
                                s.Id == long.Parse(updateSet.SessionId)
                                && s.userId == long.Parse(updateSet.UserId)
                            )
                            .Include(s => s.user)
                            .Include(s => s.user.sessions)
                            .Include(s => s.user.emailActions)
                            .Include(s => s.user.permissions)
                            .Include(s => s.user.Services)
                            .Include(s => s.user._2FAEntries),
                    true,
                    true
                );

                return session;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<DB_Session?> GetBySessionIdAsync(long sessionId)
        {
            var session = await sessionRepository.DynamicQuerySingleAsync(
                q =>
                    q.Where(s => s.Id == sessionId)
                        .Include(s => s.user)
                        .Include(s => s.user.sessions)
                        .Include(s => s.user.emailActions)
                        .Include(s => s.user.permissions)
                        .Include(s => s.user.Services)
                        .Include(s => s.user._2FAEntries),
                true,
                true
            );

            return session;
        }
    }
}
