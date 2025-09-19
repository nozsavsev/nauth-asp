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
    public class NauthController(NauthService nauthService, ServiceService serviceService, IAuthorizationService _auth, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        [Route("currentService")]
        public async Task<ActionResult<ResponseWrapper<ServiceDTO>>> CurrentService()
        {
            try
            {
                var result = await serviceService.GetByIdLoadedAsync(HttpContext.NauthService().Id!);
                return Ok(new ResponseWrapper<ServiceDTO>(WrResponseStatus.Ok, mapper.Map<ServiceDTO>(result!)));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<FullSessionDTO>(e.Status));
            }
        }

        [HttpPost]
        [Route("decodeUserToken")]
        public async Task<ActionResult<ResponseWrapper<FullSessionDTO>>> DecodeUserToken(string token)
        {
            try
            {

                var result = await nauthService.DecodeAndVerifyUserAuthToken(token);
                return Ok(new ResponseWrapper<FullSessionDTO>(WrResponseStatus.Ok, mapper.Map<FullSessionDTO>(result!)));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<FullSessionDTO>(e.Status));
            }
        }






        [HttpPost]
        [Route("createServicePermission")]
        [Authorize("ValidService")]
        public async Task<ActionResult<ResponseWrapper<PermissionDTO>>> CreateServicePermission(CreatePermissionDTO permission)
        {
            try
            {
                permission.ServiceId = HttpContext!.NauthService()!.Id!.ToString();

                var result = await nauthService.CreatePermission(permission);
                return Ok(new ResponseWrapper<PermissionDTO>(WrResponseStatus.Ok, mapper.Map<PermissionDTO>(result!)));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<PermissionDTO>(e.Status));
            }
        }

        [HttpPost]
        [Route("deleteServicePermission")]
        [Authorize("ValidService")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteServicePermission(string permissionId)
        {
            try
            {
                if ((await _auth.AuthorizeAsync(User, long.Parse(permissionId), "SerivceOwnsPermission")).Succeeded == false)
                {
                    return Forbid();
                }

                await nauthService.DeletePermission(long.Parse(permissionId));
                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<string>(e.Status));
            }
        }

        [HttpPost]
        [Route("UpdateUserPermissions")]
        public async Task<ActionResult<ResponseWrapper<string>>> GetAllServicePermissions(ServiceUpdateUserPermissionsDTO UpdateSet)
        {
            UpdateSet.permissions = UpdateSet.permissions.Where(p => _auth.AuthorizeAsync(User, p.PermissionId, "SerivceOwnsPermission").Result.Succeeded).ToList();

            try
            {
                await nauthService.UpdateUserPermissions(UpdateSet);
                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
            }
            catch (NauthException e)
            {
                return BadRequest(new ResponseWrapper<string>(e.Status));
            }
        }
    }
}
