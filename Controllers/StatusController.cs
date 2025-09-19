using Microsoft.AspNetCore.Mvc;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public ActionResult<ResponseWrapper<string>> GetStatus()
        {
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }
    }
}
