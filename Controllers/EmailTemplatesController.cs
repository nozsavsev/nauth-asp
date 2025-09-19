using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Authorize("PrManageEmailTemplates")]
    [Route("api/[controller]")]
    public class EmailTemplatesController(EmailTemplateService emailTemplateService, IMapper mapper) : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<EmailTemplateDTO>>> New(CreateEmailTemplateDTO newTemplate)
        {
            var result = await emailTemplateService.CreateEmailTemplateAsync(newTemplate);

            if (result == null)
                return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest, "Could not create email template"));
            else
                return Ok(new ResponseWrapper<EmailTemplateDTO>(WrResponseStatus.Ok, mapper.Map<EmailTemplateDTO>(result)));
        }

        [HttpPut]
        public async Task<ActionResult<ResponseWrapper<EmailTemplateDTO>>> Update(EmailTemplateDTO updatedTemplate)
        {

            var result = await emailTemplateService.UpdateEmailTemplateAsync(updatedTemplate);

            if (result == null)
                return BadRequest(new ResponseWrapper<string>(WrResponseStatus.BadRequest));
            else
                return Ok(new ResponseWrapper<EmailTemplateDTO>(WrResponseStatus.Ok, mapper.Map<EmailTemplateDTO>(result)));
        }

        [HttpDelete]
        public async Task<ActionResult<ResponseWrapper<string>>> Delete(string id)
        {
            await emailTemplateService.DeleteByidAsync(long.Parse(id));
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult<ResponseWrapper<List<EmailTemplateDTO>>>> GetAll()
        {
            var result = await emailTemplateService.GetAllAsync();
            if (result == null || !result.Any())
                return NotFound(new ResponseWrapper<string>(WrResponseStatus.NotFound));
            else
                return Ok(new ResponseWrapper<List<EmailTemplateDTO>>(WrResponseStatus.Ok, mapper.Map<List<EmailTemplateDTO>>(result)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseWrapper<EmailTemplateDTO>>> GetById(string id)
        {
            var result = await emailTemplateService.GetByIdAsync(long.Parse(id));
            if (result == null)
                return NotFound(new ResponseWrapper<string>(WrResponseStatus.NotFound));
            else
                return Ok(new ResponseWrapper<EmailTemplateDTO>(WrResponseStatus.Ok, mapper.Map<EmailTemplateDTO>(result)));
        }
    }
}
