using AutoMapper;
using IdGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Exceptions;
using nauth_asp.Models;
using nauth_asp.Repositories;
using nauth_asp.Services;
using System.Diagnostics.Eventing.Reader;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController(ServiceService serviceService, IAuthorizationService _auth, SessionRepository sessionRepository, IMapper mapper, IConfiguration config, _2FAService _2FAService) : ControllerBase
    {
        [HttpPost]
        [Route("create")]
        [Authorize("PrManageOwnServices")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<ServiceDTO>>> CreateService(CreateServiceDTO serviceDTO)
        {
            var service = await serviceService.CreateService(serviceDTO.Name, HttpContext.NauthUser().Id);
            return Ok(new ResponseWrapper<ServiceDTO>(WrResponseStatus.Ok, mapper.Map<ServiceDTO>(service)));
        }

        [HttpPost]
        [Route("update")]
        [Authorize("PrManageOwnServices")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<ServiceDTO>>> UpdateService(string id, string name)
        {
            var parsedId = long.Parse(id);
            var result = await _auth.AuthorizeAsync(User, parsedId, "UserOwnsService");
            if (!result.Succeeded) return Forbid();

            var service = await serviceService.UpdateService(parsedId, name);
            return Ok(new ResponseWrapper<ServiceDTO>(WrResponseStatus.Ok, mapper.Map<ServiceDTO>(service)));
        }


        [HttpPost]
        [Route("getSession")]
        [Authorize("PrManageOwnServices")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<string>>> IssueServiceSession(string serviceId, DateTime expiresAt)
        {
            try
            {

                var parsedId = long.Parse(serviceId);
                var result = await _auth.AuthorizeAsync(User, parsedId, "UserOwnsService");
                if (!result.Succeeded) return Forbid();

                var token = await serviceService.CreateServiceSession(parsedId, expiresAt);
                return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok, token));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest));

            }
        }


        [HttpPost]
        [Route("delete")]
        [Authorize("PrManageOwnServices")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteService(string id)
        {
            var parsedId = long.Parse(id);
            var result = await _auth.AuthorizeAsync(User, parsedId, "UserOwnsService");
            if (!result.Succeeded) return Forbid();


            await serviceService.DeleteServiceByIdAsync(parsedId);
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }

        [HttpGet]
        [Route("getAllGlobal")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<List<ServiceDTO>>>> GetAllServicesGlobal()
        {
            return Ok(new ResponseWrapper<List<ServiceDTO>>(WrResponseStatus.Ok, mapper.Map<List<ServiceDTO>>(await serviceService.GetAllAsync())));
        }

        [HttpGet]
        [Route("getbyId")]
        [Authorize("PrManageOwnServices")]
        [Authorize("PrManageServices")]
        public async Task<ActionResult<ResponseWrapper<ServiceDTO>>> GetById(string serviceId)
        {
            var parsedId = long.Parse(serviceId);
            var result = await _auth.AuthorizeAsync(User, parsedId, "UserOwnsService");
            if (!result.Succeeded) return Forbid();
            var service = await serviceService.GetByIdLoadedAsync(parsedId);

            if (service != null)
                return Ok(new ResponseWrapper<ServiceDTO>(WrResponseStatus.Ok, mapper.Map<ServiceDTO>(service)));
            else
                return NotFound(new ResponseWrapper<ServiceDTO>(WrResponseStatus.NotFound));
        }

        [HttpGet]
        [Route("getAllOwned")]
        [Authorize("PrManageOwnServices")]
        public async Task<ActionResult<ResponseWrapper<List<ServiceDTO>>>> GetAllServicesOwned()
        {
            return Ok(new ResponseWrapper<List<ServiceDTO>>(WrResponseStatus.Ok,
            mapper.Map<List<ServiceDTO>>(await serviceService.GetAllOwnedAsync(HttpContext.NauthUser()!.Id))));
        }


    }
}
