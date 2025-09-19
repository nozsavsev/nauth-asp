using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("PrManageUsers")]
    public class UserManagementController(UserService userService, PermissionService permissionService, SessionService sessionService, IMapper mapper, EmailActionService emailActionService) : ControllerBase
    {
        [HttpGet]
        [Route("fetchPermissions")]
        public async Task<ActionResult<ResponseWrapper<List<PermissionDTO>>>> FetchPermissions()
        {
            var premission = await permissionService.GetAll();
            return Ok(new ResponseWrapper<List<PermissionDTO>>(WrResponseStatus.Ok, mapper.Map<List<PermissionDTO>>(premission)));
        }

        [HttpGet]
        [Route("fetchUsers")]
        public async Task<ActionResult<ResponseWrapper<List<UserDTO>>>> FetchUsers(string? match, int skip = 0, int take = 20)
        {
            var users = await userService.AdminGetUsers(match, skip, take);
            return Ok(new ResponseWrapper<List<UserDTO>>(WrResponseStatus.Ok, mapper.Map<List<UserDTO>>(users)));
        }

        [HttpPost]
        [Route("updatePermissions")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> UpdatePermissions(UserDTO userDTO)
        {
            var permissionIds = userDTO.Permissions.Select(p => long.Parse(p.PermissionId)).ToList();
            var updatedUser = await userService.UpdatePermissions(long.Parse(userDTO.Id), permissionIds);
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok, mapper.Map<UserDTO>(updatedUser)));
        }

        [HttpPost]
        [Route("deleteUser")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> DeleteUser(string id)
        {
            await userService.DeleteAsync(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("forceVerifyEmail")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> ForceVerifyEmail(string id)
        {
            await userService.VerifyEmailAsync(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("forceUnVerifyEmail")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> ForceUnVerifyEmail(string id)
        {
            await userService.UnVerifyEmailAsync(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }


        [HttpPost]
        [Route("disableUser")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> DisableUser(string id)
        {
            await userService.DisableUserAsync(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("enableUser")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> EnableUser(string id)
        {
            await userService.EnableUserAsync(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("setUserEmail")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> SetUserEmail(string id, string email)
        {
            await userService.SetUserEmailAsync(long.Parse(id), email);
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("setUserPassword")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> SetUserPassword(string id, string password)
        {
            await userService.SetUserPassword(long.Parse(id), password);
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("revokeAllUserSessions")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> RevokeAllUserSessions(string id)
        {
            await sessionService.RevokeAllSessions(long.Parse(id));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }

        [HttpPost]
        [Route("updateUserName")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> UpdateUserName(AdminUpdateUserNameDTO dto)
        {
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok, mapper.Map<UserDTO>(await userService.UpdateUserName(long.Parse(dto.Id), dto.Name))));
        }

        [HttpPost]
        [Route("neutralizeEmailAction")]
        public async Task<ActionResult<ResponseWrapper<NeutralizeEmailActionResponse>>> NeutralizeEmailAction(string id)
        {
            var parsedId = long.Parse(id);
            await emailActionService.UniversalNeutralize(parsedId);
            return Ok(new ResponseWrapper<NeutralizeEmailActionResponse>(WrResponseStatus.Ok, new NeutralizeEmailActionResponse(true)));
        }

        private async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> HandleEmailActionRequest(Func<Task<int>> request)
        {
            var response = await request();
            switch (response)
            {
                case > 0:
                    return Ok(new ResponseWrapper<RequestEmailActionResponse>(WrResponseStatus.Cooldown, new RequestEmailActionResponse(false, response)));
                case 0:
                    return Ok(new ResponseWrapper<RequestEmailActionResponse>(WrResponseStatus.Ok, new RequestEmailActionResponse(true)));
                default:
                    return BadRequest(new ResponseWrapper<RequestEmailActionResponse>(WrResponseStatus.BadRequest, new RequestEmailActionResponse(false)));
            }
        }

        [HttpPost]
        [Route("verifyEmailRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> VerifyEmailRequest(string userId)
        {
            var user = await userService.GetByIdAsync(long.Parse(userId), tracking: false);
            return await HandleEmailActionRequest(() => emailActionService.VerifyEmail_Request(user!));
        }

        [HttpPost]
        [Route("passwordResetRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> PasswordResetRequest(string userId)
        {
            var user = await userService.GetByIdAsync(long.Parse(userId), tracking: false);
            return await HandleEmailActionRequest(() => emailActionService.PasswordReset_Request(user!));
        }

        [HttpPost]
        [Route("deleteAccountRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> DeleteAccountRequest(string userId)
        {
            var user = await userService.GetByIdAsync(long.Parse(userId), tracking: false);
            return await HandleEmailActionRequest(() => emailActionService.DeleteAccount_Request(user!));
        }

        [HttpPost]
        [Route("changeEmailRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> ChangeEmailRequest(string userId, string newEmail)
        {
            var user = await userService.GetByIdAsync(long.Parse(userId), tracking: false);
            return await HandleEmailActionRequest(() => emailActionService.ChangeEmail_Request(user!, newEmail));
        }
    }
}
