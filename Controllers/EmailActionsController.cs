using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nauth_asp.Models;
using nauth_asp.Services;

namespace nauth_asp.Controllers
{


    public class RequestEmailActionResponse
    {
        public RequestEmailActionResponse(bool sentSuccessfully, int? secondsToWait = 0)
        {
            this.sentSuccessfully = sentSuccessfully;
            this.secondsToWait = secondsToWait;
        }

        public bool sentSuccessfully { get; set; } = false;
        public int? secondsToWait { get; set; } = 0;
    }

    public class NeutralizeEmailActionResponse
    {
        public NeutralizeEmailActionResponse(bool neutralizedSuccessfully = false)
        {
            this.neutralizedSuccessfully = neutralizedSuccessfully;
        }

        public bool neutralizedSuccessfully { get; set; } = false;
    }


    public class ApplyEmailActionResponse
    {
        public ApplyEmailActionResponse(bool appliedSuccessfully = false)
        {
            this.appliedSuccessfully = appliedSuccessfully;
        }

        public bool appliedSuccessfully { get; set; } = false;
    }



    [ApiController]
    [Route("api/[controller]")]
    public class EmailActionsController(EmailActionService emailActionService, UserService userService, IMapper mapper, IAuthorizationService _auth) : ControllerBase
    {
        [HttpPost]
        [Route("decodeApplyToken")]
        public async Task<ActionResult<ResponseWrapper<DecodedEmailActionDTO>>> DecodeApplyToken(string token)
        {
            var emailAction = await emailActionService.DecodeApplyToken(token);
            return Ok(new ResponseWrapper<DecodedEmailActionDTO>(WrResponseStatus.Ok, mapper.Map<DecodedEmailActionDTO>(emailAction)));
        }

        [HttpPost]
        [Route("neutralizeEmailAction")]
        public async Task<ActionResult<ResponseWrapper<NeutralizeEmailActionResponse>>> NeutralizeEmailAction(string id)
        {
            var parsedId = long.Parse(id);
            var result = await _auth.AuthorizeAsync(User, parsedId, "UserOwnsEmailAction");
            if (!result.Succeeded) return Forbid();

            await emailActionService.UniversalNeutralize(parsedId);
            return Ok(new ResponseWrapper<NeutralizeEmailActionResponse>(WrResponseStatus.Ok, new NeutralizeEmailActionResponse(true)));
        }


        [HttpGet]
        [Authorize("allowNoEmail")]
        [Route("getAllByUserId")]
        public async Task<ActionResult<ResponseWrapper<List<EmailActionDTO>>>> GetAllByUserId()
        {
            var user = HttpContext.NauthUser();
            if (user == null)
            {
                return Unauthorized();
            }
            var emailActions = await emailActionService.GetAllByUserIdAsync(user.Id);
            var converted = mapper.Map<List<EmailActionDTO>>(emailActions);
            return Ok(new ResponseWrapper<List<EmailActionDTO>>(WrResponseStatus.Ok, converted));
        }

        #region VerifyEmail

        [HttpPost]
        [Authorize("allowNoEmail")]
        [Route("verifyEmailRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> VerifyEmailRequest()
        {

            var user = HttpContext.NauthUser();

            var response = await emailActionService.VerifyEmail_Request(user!);

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
        [Route("applyEmailRequest")]
        public async Task<ActionResult<ResponseWrapper<ApplyEmailActionResponse>>> ApplyEmailRequest(string token)
        {

            var response = await emailActionService.VerifyEmail_Apply(token);

            switch (response)
            {
                case 0:
                    return Ok(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.Ok, new ApplyEmailActionResponse(true)));
                default:
                    return BadRequest(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.BadRequest, new ApplyEmailActionResponse(false)));
            }

        }

        #endregion

        #region PasswordReset
        [HttpPost]
        [Route("passwordResetRequest")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> PasswordResetRequest(string? email)
        {

            var user = email != null ? await userService.GetByEmailAsync(email) : HttpContext.NauthUser();

            var response = await emailActionService.PasswordReset_Request(user!);

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
        [Route("passwordResetApply")]
        public async Task<ActionResult<ResponseWrapper<ApplyEmailActionResponse>>> PasswordResetApply(string token, string password)
        {

            var response = await emailActionService.PasswordReset_Apply(token, password, HttpContext.NauthSession()?.Id);

            switch (response)
            {
                case 0:
                    return Ok(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.Ok, new ApplyEmailActionResponse(true)));
                default:
                    return BadRequest(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.BadRequest, new ApplyEmailActionResponse(false)));
            }
        }

        #endregion

        #region EmailCode
        [HttpPost]
        [Route("emailCodeRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> EmailCodeRequest()
        {

            var user = HttpContext.NauthUser();

            var response = await emailActionService.EmailCode_Request(user!);

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

        #endregion

        #region EmailSignIn
        [HttpPost]
        [Route("emailSignInRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> EmailSignInRequest(string email)
        {

            var response = await emailActionService.EmailSignIn_Request(email);

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

        #endregion

        #region DeleteAccount
        [HttpPost]
        [Authorize("allowNoEmail")]
        [Route("deleteAccountRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> DeleteAccountRequest()
        {
            var user = HttpContext.NauthUser();
            var response = await emailActionService.DeleteAccount_Request(user!);
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
        [Route("deleteAccountApply")]
        public async Task<ActionResult<ResponseWrapper<ApplyEmailActionResponse>>> DeleteAccountApply(string token)
        {
            var response = await emailActionService.DeleteAccount_Apply(token);
            switch (response)
            {
                case 0:
                    return Ok(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.Ok, new ApplyEmailActionResponse(true)));
                default:
                    return BadRequest(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.BadRequest, new ApplyEmailActionResponse(false)));
            }
        }

        #endregion

        #region ChangeEmail
        [HttpPost]
        [Authorize("allowNoEmail")]
        [Route("changeEmailRequest")]
        public async Task<ActionResult<ResponseWrapper<RequestEmailActionResponse>>> ChangeEmailRequest(string newEmail)
        {
            var user = HttpContext.NauthUser();
            var response = await emailActionService.ChangeEmail_Request(user!, newEmail);

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
        [Route("changeEmailApply")]
        public async Task<ActionResult<ResponseWrapper<ApplyEmailActionResponse>>> ChangeEmailApply(string token)
        {
            var response = await emailActionService.ChangeEmail_Apply(token);
            switch (response)
            {
                case 0:
                    return Ok(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.Ok, new ApplyEmailActionResponse(true)));
                default:
                    return BadRequest(new ResponseWrapper<ApplyEmailActionResponse>(WrResponseStatus.BadRequest, new ApplyEmailActionResponse(false)));
            }
        }

        #endregion

    }
}
