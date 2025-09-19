using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController(SessionService sessionService, IMapper mapper, IAuthorizationService auth) : ControllerBase
    {

        [HttpDelete]
        [Authorize("allowNo2FA")]
        [Route("revokeCurrent/")]
        public async Task<ActionResult<ResponseWrapper<string>>> RevokeCurrentSession()
        {
          var  sessionId = HttpContext.NauthSession().Id;

            await sessionService.RevokeSessionAsync(sessionId);
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }


        [HttpDelete]
        [Authorize("allowNoEmail")]
        [Route("revoke/")]
        public async Task<ActionResult<ResponseWrapper<string>>> RevokeSession(string? sessionId)
        {
            sessionId = sessionId ?? HttpContext.NauthSession().Id.ToString();

            var session = long.Parse(sessionId);

            var result = await auth.AuthorizeAsync(User, session, "UserOwnsSession");
            if (!result.Succeeded) return Forbid();

            await sessionService.RevokeSessionAsync(session);
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }

        [HttpDelete]
        [Authorize("allowNoEmail")]
        [Route("revoke/my")]
        public async Task<ActionResult<ResponseWrapper<string>>> RevokeMySession()
        {
            var sessionId = HttpContext.NauthSession()!.Id;
            await sessionService.RevokeSessionAsync(sessionId);
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }

        [HttpDelete]
        [Authorize("allowNoEmail")]
        [Route("revoke/allMy")]
        public async Task<ActionResult<ResponseWrapper<string>>> RevokeAllMySessions()
        {
            await sessionService.RevokeAllSessions(HttpContext.NauthUser()!.Id, HttpContext.NauthSession()!.Id);
            return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
        }

        [HttpGet]
        [Authorize("allowNoEmail")]
        [Route("getMy")]
        public async Task<ActionResult<ResponseWrapper<List<SessionDTO>>>> GetMySessions()
        {
            var sessions = await sessionService.GetByUserIdAsync(HttpContext.NauthUser()!.Id);
            return Ok(new ResponseWrapper<List<SessionDTO>>(WrResponseStatus.Ok, mapper.Map<List<SessionDTO>>(sessions)));
        }

    }
}
