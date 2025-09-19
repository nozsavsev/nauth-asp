using Microsoft.EntityFrameworkCore;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;

namespace nauth_asp.Services
{
    public class EmailActionService : GenericService<DB_EmailAction>
    {
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly UserService _userService;
        private readonly ApplyTokenProvider _applyTokenProvider;
        private readonly IServiceProvider _serviceProvider;

        public EmailActionService(EmailActionRepository repository, ApplyTokenProvider applyTokenProvider, IConfiguration config, IServiceProvider serviceProvider, IEmailService emailService, EmailTemplateService emailTemplateService) : base(repository)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _applyTokenProvider = applyTokenProvider;
            _emailService = emailService;
            _userService = serviceProvider.GetRequiredService<UserService>();
            _emailTemplateService = emailTemplateService;
        }


        public async Task<List<DB_EmailAction>> GetAllByUserIdAsync(long userId)
        {
            return await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == userId), loadDependencies: false);
        }


        public async Task<DB_EmailAction?> DecodeApplyToken(string ApplyToken)
        {
            var action = _applyTokenProvider.DecodeAndVerify(ApplyToken);
            if (action == null)
                return null;
            return await _repository.DynamicQuerySingleAsync(q => q.Where(ea => ea.Id == action.Id).IgnoreAutoIncludes().Include(ea => ea.User));
        }


        public async Task<int> UniversalNeutralize(long id)
        {

            try
            {
                await _repository.DeleteByIdAsync(id);
            }
            catch
            {

            }

            return 0;
        }

        #region VerifyEmail

        /// <returns>
        /// -1 bad request, 
        /// 0 sent,
        /// >0 cooldown
        /// </returns>
        public async Task<int> VerifyEmail_Request(DB_User user)
        {
            if (user == null)
                return -1;

            if (user.isEmailVerified)
                return -1;

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.VerifyEmail).OrderByDescending(ea => ea.CreatedAt));

            if (emailActions.Count > 0)
            {
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff
            }

            var lastEmailAction = emailActions.FirstOrDefault();

            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                lastEmailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.VerifyEmail,
                                     new { link = CreateActionUri(_config["Frontend:EmailVerificationPath"]!, lastEmailAction.token) });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {
                //create new email action
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.VerifyEmail;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);
                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.VerifyEmail,
                                     new { link = CreateActionUri(_config["Frontend:EmailVerificationPath"]!, emailAction.token) });
                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);
                return 0;
            }
        }

        public async Task<int> VerifyEmail_Apply(string token)
        {

            var action = _applyTokenProvider.DecodeAndVerify(token);
            if (action == null)
                return -1;

            await _repository.DeleteByIdAsync(action.Id);
            await _userService.VerifyEmailAsync(action.userId);

            return 0;

        }

        #endregion

        #region PasswordReset

        public async Task<int> PasswordReset_Request(DB_User user)
        {
            if (user == null)
                return -1;

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.PasswordReset).OrderByDescending(ea => ea.CreatedAt));

            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff

            var lastEmailAction = emailActions.FirstOrDefault();

            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                lastEmailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.PasswordReset,
                                     new { link = CreateActionUri(_config["Frontend:PasswordResetPath"]!, lastEmailAction.token) });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {

                //create new email action
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.PasswordReset;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);
                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.PasswordReset,
                                     new { link = CreateActionUri(_config["Frontend:PasswordResetPath"]!, emailAction.token) });
                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);
                return 0;
            }
        }

        public async Task<int> PasswordReset_Apply(string token, string password, long? currentSessionId = null)
        {
            var action = _applyTokenProvider.DecodeAndVerify(token);
            if (action == null)
                return -1;
            await _repository.DeleteByIdAsync(action.Id);
            await _userService.SetUserPassword(action.userId, password, currentSessionId);
            return 0;
        }

        #endregion

        #region EmailCode
        public async Task<int> EmailCode_Request(DB_User user)
        {
            if (user == null)
                return -1;


            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.EmailCode).OrderByDescending(ea => ea.CreatedAt));

            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff

            var lastEmailAction = emailActions.FirstOrDefault();

            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                lastEmailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.EmailCode,
                                     new { code = lastEmailAction.data });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {
                //create new email action
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.EmailCode;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.data = new Random().Next(0, 999999).ToString("D6");
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.EmailCode,
                                     new { code = emailAction.data });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
        }

        public async Task<bool> EmailCode_Verify(string code, DB_User user)
        {

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.EmailCode).OrderByDescending(ea => ea.CreatedAt));

            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff

            var lastEmailAction = emailActions.FirstOrDefault();

            if (lastEmailAction != null && lastEmailAction.data == code && lastEmailAction.ExpiresAt > DateTime.UtcNow)
            {
                await _repository.DeleteByIdAsync(lastEmailAction.Id);
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region EmailSignIn

        public async Task<int> EmailSignIn_Request(string email)
        {
            if (email == null)
                return -1;

            var user = await _userService.GetByEmailAsync(email);

            if (user == null)
                return -1;

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.EmailSignIn).OrderByDescending(ea => ea.CreatedAt));

            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff

            var lastEmailAction = emailActions.FirstOrDefault();

            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                lastEmailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.EmailSignIn,
                                     new { code = lastEmailAction.data });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {
                //create new email action
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.EmailSignIn;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.data = new Random().Next(0, 999999).ToString("D6");
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.EmailSignIn,
                                     new { code = emailAction.data });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
        }

        public async Task<bool> EmailSignIn_Verify(string code, DB_User user)
        {
            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.EmailSignIn).OrderByDescending(ea => ea.CreatedAt));
            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff
            var lastEmailAction = emailActions.FirstOrDefault();
            if (lastEmailAction != null && lastEmailAction.data == code && lastEmailAction.ExpiresAt > DateTime.UtcNow)
            {
                await _repository.DeleteByIdAsync(lastEmailAction.Id);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region DeleteAccount

        public async Task<int> DeleteAccount_Request(DB_User user)
        {
            if (user == null)
                return -1;

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.DeleteAccount).OrderByDescending(ea => ea.CreatedAt));
            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff
            var lastEmailAction = emailActions.FirstOrDefault();
            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.DeleteAccount,
                                     new { activate_link = CreateActionUri(_config["Frontend:DeleteAccountPath"]!, lastEmailAction.token),
                                        neutralize_link = CreateActionUri(_config["Frontend:DeleteAccountPath"]!, lastEmailAction.token) });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.DeleteAccount;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.DeleteAccount,
                                     new
                                     {
                                         activate_link = CreateActionUri(_config["Frontend:DeleteAccountPath"]!, emailAction.token),
                                         neutralize_link = CreateActionUri(_config["Frontend:DeleteAccountPath"]!, emailAction.token)
                                     });

                await _emailService.SendEmailAsync(user.email, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
        }

        public async Task<int> DeleteAccount_Apply(string token)
        {
            var action = _applyTokenProvider.DecodeAndVerify(token);
            if (action == null)
                return -1;
            await _repository.DeleteByIdAsync(action.Id);
            await _userService.DeleteAsync(action.userId);
            return 0;
        }

        #endregion

        #region ChangeEmail
        public async Task<int> ChangeEmail_Request(DB_User user, string newEmail)
        {

            if (user == null)
                return -1;

            var emailActions = await _repository.DynamicQueryManyAsync(q => q.Where(ea => ea.userId == user.Id && ea.type == EmailActionType.ChangeEmail).OrderByDescending(ea => ea.CreatedAt));
            if (emailActions.Count > 0)
                await _repository.DeleteManyAsync(emailActions.Skip(1).ToList()); //cleanup old stuff
            var lastEmailAction = emailActions.FirstOrDefault();
            if (lastEmailAction != null)
            {
                //check that user is not spamming
                if (lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) > DateTime.UtcNow)
                    return (int)(lastEmailAction.CreatedAt.AddSeconds(int.Parse(_config["JWT:emailActionCooldownSeconds"]!)) - DateTime.UtcNow).TotalSeconds;

                //now update the last email action to be sent now and send it again
                lastEmailAction.CreatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(lastEmailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.ChangeEmail,
                                     new
                                     {

                                         activate_link = CreateActionUri(_config["Frontend:ChangeEmailPath"]!, lastEmailAction.token),
                                         neutralize_link = CreateActionUri(_config["Frontend:ChangeEmailPath"]!, lastEmailAction.token)

                                     });

                await _emailService.SendEmailAsync(newEmail, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }
            else
            {
                var emailAction = new DB_EmailAction();
                emailAction.userId = user.Id;
                emailAction.type = EmailActionType.ChangeEmail;
                emailAction.token = _applyTokenProvider.Generate(emailAction.Id.ToString());
                emailAction.data = newEmail;
                emailAction.CreatedAt = DateTime.UtcNow;
                emailAction.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:emailActionExpiresMinutes"]!));
                await _repository.AddAsync(emailAction);

                var template = await _emailTemplateService.PopulateEmailTemplateAsync(EmailTemplateType.ChangeEmail,
                                     new
                                     {
                                         activate_link = CreateActionUri(_config["Frontend:ChangeEmailPath"]!, emailAction.token),
                                         neutralize_link = CreateActionUri(_config["Frontend:ChangeEmailPath"]!, emailAction.token)
                                     });

                await _emailService.SendEmailAsync(newEmail, template!.Subject, template.Body, template.HtmlBody);

                return 0;
            }

        }

        public async Task<int> ChangeEmail_Apply(string token)
        {
            var action = _applyTokenProvider.DecodeAndVerify(token);
            if (action == null)
                return -1;

            //invalidate all email actions for this account
            await _repository.DeleteManyAsync(await GetAllByUserIdAsync(action.userId));

            await _userService.SetUserEmailAsync(action.userId, action.data!);
            return 0;
        }

        #endregion

        private Uri CreateActionUri(string path, string token)
        {
            return new Uri(new Uri(_config["Frontend:BaseUrl"]!), path + "?token=" + token);
        }
    }
}


