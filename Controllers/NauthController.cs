using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Exceptions;
using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ValidService")]
    public class NauthController(NauthService nauthService, UserService userService, ServiceService serviceService, IAuthorizationService _auth, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        [Route("currentService")]
        public async Task<ActionResult<ResponseWrapper<ServiceDTO>>> CurrentService()
        {
            Console.WriteLine("Getting current service");

            try
            {
                var result = await serviceService.GetByIdLoadedAsync(HttpContext.NauthService().Id!);
                return Ok(new ResponseWrapper<ServiceDTO>(WrResponseStatus.Ok, mapper.Map<ServiceDTO>(result!)));
            }
            catch (NauthException e)
            {
                Console.WriteLine(e);
                return BadRequest(new ResponseWrapper<FullSessionDTO>(e));
            }
        }

        [HttpPost]
        [Route("decodeUserToken")]
        public async Task<ActionResult<ResponseWrapper<FullSessionDTO>>> DecodeUserToken(string token)
        {
            Console.WriteLine("Decoding user token");
            try
            {

                var result = await nauthService.DecodeAndVerifyUserAuthToken(token);
                return Ok(new ResponseWrapper<FullSessionDTO>(WrResponseStatus.Ok, mapper.Map<FullSessionDTO>(result!)));
            }
            catch (NauthException e)
            {
                Console.WriteLine(e);
                return BadRequest(new ResponseWrapper<FullSessionDTO>(e));
            }
        }


        [HttpPost]
        [Route("fetchUsers")]
        public async Task<ActionResult<ResponseWrapper<List<UserDTO>>>> FetchUsers(string? match, int skip = 0, int take = 20)
        {
            var users = await userService.AdminGetUsers(match, skip, take);
            return Ok(new ResponseWrapper<List<UserDTO>>(WrResponseStatus.Ok, mapper.Map<List<UserDTO>>(users)));
        }

        [HttpPost]
        [Route("getUserById")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> GetUserById(string userId)
        {
            var users = await userService.GetByIdAsync(long.Parse(userId));
            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok, mapper.Map<UserDTO>(users)));
        }

        [HttpPost]
        [Route("getUserBySessionId")]
        public async Task<ActionResult<ResponseWrapper<FullSessionDTO>>> GetUserBySessionId(string sessionId)
        {
            var users = await nauthService.GetBySessionIdAsync(long.Parse(sessionId));
            return Ok(new ResponseWrapper<FullSessionDTO>(WrResponseStatus.Ok, mapper.Map<FullSessionDTO>(users)));
        }

        [HttpPost]
        [Route("createServicePermission")]
        [Authorize("ValidService")]
        public async Task<ActionResult<ResponseWrapper<PermissionDTO>>> CreateServicePermission(CreatePermissionDTO permission)
        {
            Console.WriteLine("Creating service permission");
            try
            {
                permission.ServiceId = HttpContext!.NauthService()!.Id!.ToString();
                var result = await nauthService.CreatePermission(permission);
                return Ok(new ResponseWrapper<PermissionDTO>(WrResponseStatus.Ok, mapper.Map<PermissionDTO>(result!)));
            }
            catch (NauthException e)
            {
                Console.WriteLine(e);
                return BadRequest(new ResponseWrapper<PermissionDTO>(e));
            }
        }

        [HttpPost]
        [Route("deleteServicePermission")]
        [Authorize("ValidService")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteServicePermission(string permissionId)
        {
            Console.WriteLine("Deleting service permission");
            try
            {
                if ((await _auth.AuthorizeAsync(User, long.Parse(permissionId), "ServiceOwnsPermission")).Succeeded == false)
                {
                    return Forbid();
                }

                await nauthService.DeletePermission(long.Parse(permissionId));
                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
            }
            catch (NauthException e)
            {
                Console.WriteLine(e);
                return BadRequest(new ResponseWrapper<string>(e ));
            }
        }

        [HttpPost]
        [Route("updateUserPermissions")]
        public async Task<ActionResult<ResponseWrapper<FullSessionDTO>>> UpdateUserPermissions(ServiceUpdateUserPermissionsDTO UpdateSet)
        {
            Console.WriteLine("Updating user permissions");
            UpdateSet.permissions = UpdateSet.permissions.Where(p => 
            _auth.AuthorizeAsync(User, long.Parse(p.PermissionId), "ServiceOwnsPermission").Result.Succeeded
            ).ToList();

            try
            {
                var fullSession = await nauthService.UpdateUserPermissions(UpdateSet);
                return Ok(new ResponseWrapper<FullSessionDTO>(WrResponseStatus.Ok, mapper.Map<FullSessionDTO>(fullSession)));
            }
            catch (NauthException e)
            {
                Console.WriteLine(e);
                return BadRequest(new ResponseWrapper<FullSessionDTO>(e));
            }
        }
    }
}
