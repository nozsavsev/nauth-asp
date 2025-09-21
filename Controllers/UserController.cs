using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using nauth_asp.Services;
using nauth_asp.Services.ObjectStorage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using UAParser;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserService userService, SessionService sessionService, SessionRepository sessionRepository, IMapper mapper, IConfiguration config, _2FAService _2FAService, IObjectStorageService objectStorageService) : ControllerBase
    {
        private readonly string _avatarBucketName = config["Amazon:avatarBucketName"]!;
        private readonly string _avatarPublicUrl = config["Amazon:avatarPublicUrl"]!;

        [HttpGet]
        [Authorize("allowNoEmail")]
        [Route("currentUser")]
        public async Task<ActionResult<ResponseWrapper<UserDTO?>>> GetCurrentUser()
        {
            var user = HttpContext.NauthUser();
            user.sessions = user.sessions.Where(s => s.serviceId == null).ToList();
            return Ok(new ResponseWrapper<UserDTO?>(WrResponseStatus.Ok, mapper.Map<UserDTO>(HttpContext.NauthUser())));
        }

        [HttpPost]
        [Route("uploadAvatar")]
        [Authorize("allowNoEmail")]
        public async Task<ActionResult<ResponseWrapper<UserDTO?>>> UploadAvatar(IFormFile? file)
        {
            try
            {

                var user = HttpContext.NauthUser();
                if (user == null)
                {
                    return Unauthorized(new ResponseWrapper<string>(WrResponseStatus.Unauthorized));
                }

                if (file == null || file.Length == 0)
                {
                    if (!string.IsNullOrEmpty(user.AvatarURL))
                    {
                        var deletionKey = KeyGenerators.GetUserAvatarKey(user.Id);
                        await objectStorageService.DeleteFileAsync(_avatarBucketName, deletionKey);
                        var updatedUser = await userService.SetAvatarUrlAsync(user.Id, null);
                        return Ok(new ResponseWrapper<UserDTO?>(WrResponseStatus.Ok, mapper.Map<UserDTO>(updatedUser)));
                    }
                    return Ok(new ResponseWrapper<UserDTO?>(WrResponseStatus.Ok, mapper.Map<UserDTO>(user)));
                }

                if (file.Length > 5 * 1024 * 1024) // 5 MB
                {
                    return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest, "File size must be less than 5MB."));
                }

                var key = KeyGenerators.GetUserAvatarKey(user.Id);

                using var image = await Image.LoadAsync(file.OpenReadStream());
                image.Mutate(x => x.Resize(512, 512));

                using var ms = new MemoryStream();
                await image.SaveAsPngAsync(ms);
                ms.Position = 0;

                var success = await objectStorageService.UploadFileAsync(_avatarBucketName, key, ms, "image/png");

                if (success)
                {
                    var avatarUrl = $"{_avatarPublicUrl}/{key}";
                    var updatedUser = await userService.SetAvatarUrlAsync(user.Id, avatarUrl);
                    return Ok(new ResponseWrapper<UserDTO?>(WrResponseStatus.Ok, mapper.Map<UserDTO>(updatedUser)));
                }
                else
                {
                    return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest, "Failed to upload avatar."));
                }
            }
            catch (NauthException ex)
            {
                return BadRequest(new ResponseWrapper<string>(ex.Status));
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<ResponseWrapper<UserDTO?>>> Register(CreateUserDTO userCreateDTO)
        {
            try
            {

                var userToken = await userService.RegisterWithEmailAndPasswordAsync(userCreateDTO);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!)),
                    Domain = config["Frontend:CookieDomain"]
                };

                Response.Cookies.Append(config["JWT:Cookiekey"]!, userToken.token, cookieOptions);

                return Ok(new ResponseWrapper<UserDTO?>(WrResponseStatus.Ok, mapper.Map<UserDTO>(userToken.user)));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<UserDTO?>(e.Status));
            }

        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<ResponseWrapper<string?>>> LoginWithPassword(CreateUserDTO userCreateDTO)
        {
            try
            {

                var userToken = await userService.LoginWithEmailAndPasswordAsync(userCreateDTO);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!)),
                    Domain = config["Frontend:CookieDomain"]
                };

                Response.Cookies.Append(config["JWT:Cookiekey"]!, userToken.token, cookieOptions);

                return Ok(new ResponseWrapper<string?>(WrResponseStatus.Ok));
            }
            catch (NauthException ex)
            {
                return BadRequest(new ResponseWrapper<string>(ex.Status));
            }
        }



        [HttpGet]
        [Route("ContinueWithGoogle")]
        public async Task<ActionResult<ResponseWrapper<string>>> ContinueWithGoogle(string GoogleAccessToken)
        {
            try
            {
                var userToken = await userService.ContinueWithGoogle(GoogleAccessToken);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!)),
                    Domain = config["Frontend:CookieDomain"]
                };

                Response.Cookies.Append(config["JWT:Cookiekey"]!, userToken.token, cookieOptions);

                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(new ResponseWrapper<string>(WrResponseStatus.InternalError));
            }
        }


        [HttpPost]
        [Route("loginWithCode")]
        public async Task<ActionResult<ResponseWrapper<string?>>> LoginWithCode(string email, string code)
        {
            try
            {

                var userToken = await userService.LoginWithEmailAndCodeAsync(email, code);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(int.Parse(config["JWT:expiresAfterDays"]!)),
                    Domain = config["Frontend:CookieDomain"]
                };

                Response.Cookies.Append(config["JWT:Cookiekey"]!, userToken.token, cookieOptions);

                return Ok(new ResponseWrapper<string?>(WrResponseStatus.Ok));

            }
            catch (NauthException ex)
            {
                return BadRequest(new ResponseWrapper<string>(ex.Status));
            }
        }


        [HttpPost]
        [Route("setup2FA")]
        [Authorize("allowNoEmail")]
        public async Task<ActionResult<ResponseWrapper<_2FAEntrySetupDTO>>> Setup2FA(string code, string name)
        {
            try
            {

                var user = HttpContext.NauthUser();
                if (user == null) return Unauthorized(new ResponseWrapper<string>(WrResponseStatus.Unauthorized));
                var setup = await _2FAService.CreateAsync(code, name, user);

                return Ok(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.Ok, setup));
            }
            catch (NauthException ex)
            {
                return BadRequest(new ResponseWrapper<string>(ex.Status));
            }
        }





        [HttpPost]
        [Route("activate2FA")]
        [Authorize("allowNoEmail")]
        public async Task<ActionResult<ResponseWrapper<string>>> Activate2FA(string code, string _2faId)
        {
            try
            {
                await _2FAService.ActivateAsync(code, long.Parse(_2faId), HttpContext.NauthUser()!.Id);
                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
            }
            catch
            {

                return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest));
            }
        }


        [HttpPost]
        [Route("delete2FAWithRecoveryCode")]
        [Authorize("allowNo2FA")]
        public async Task<ActionResult<ResponseWrapper<_2FAEntrySetupDTO>>> Delete2FAWithRecoveryCode(string code)
        {
            try
            {
                var setup = await _2FAService.DeleteWithRecoveryCodeAsync(code, HttpContext.NauthUser()!);
                return Ok(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.Ok, setup));
            }
            catch (NauthException)
            {
                return BadRequest(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.BadRequest));
            }
        }

        [HttpPost]
        [Route("activateSession")]
        [Authorize("allowNo2FA")]
        public async Task<ActionResult<ResponseWrapper<_2FAEntrySetupDTO>>> ActivateSession(string code)
        {
            try
            {

                var session = HttpContext.NauthSession();
                if (session == null) return Unauthorized(new ResponseWrapper<string>(WrResponseStatus.Unauthorized));
                var id = session.Id;

                var fullSession = await sessionRepository.GetByIdAsync(id, (s) => s.IgnoreAutoIncludes().Include(ss => ss.user!._2FAEntries));
                if (fullSession == null) return Unauthorized(new ResponseWrapper<string>(WrResponseStatus.Unauthorized));

                await _2FAService.VerifySessionAsync(fullSession, code);

                return Ok(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.Ok));
            }
            catch
            {
                return BadRequest(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.BadRequest));
            }
        }


        [HttpPost]
        [Route("delete2FAWithCode")]
        [Authorize("allowNoEmail")]
        public async Task<ActionResult<ResponseWrapper<_2FAEntrySetupDTO>>> Delete2FAWithCode(string code, string _2faId)
        {
            try
            {

                await _2FAService.DeleteAsync(long.Parse(_2faId), code, HttpContext.NauthUser()!);

                return Ok(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.Ok));
            }
            catch (NauthException)
            {
                return BadRequest(new ResponseWrapper<_2FAEntrySetupDTO>(WrResponseStatus.BadRequest));
            }
        }


        [HttpPost]
        [Route("updateUserName")]
        [Authorize("allowNoEmail")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> UpdateUserName(UpdateNameDTO dto)
        {
            try
            {

                var userId = HttpContext.NauthUser()!.Id;
                return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok, mapper.Map<UserDTO>(await userService.UpdateUserName(userId, dto.Name))));
            }
            catch (NauthException ex)
            {
                return BadRequest(new ResponseWrapper<string>(ex.Status));
            }
        }
    }
}
