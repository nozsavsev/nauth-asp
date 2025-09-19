using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using System.Reflection.Metadata.Ecma335;

namespace nauth_asp.Services
{
    public class _2FAService : GenericService<DB_2FAEntry>
    {
        private readonly _2FARepository _twoFactorAuthRepository;
        private readonly EmailActionService _emailActionService;
        private readonly SessionRepository _sessionRepository;
        private readonly SessionService _sessionService;
        private readonly UserRepository _userRepository;

        public _2FAService(UserRepository userRepository, _2FARepository twoFactorAuthRepository, EmailActionService emailActionService, SessionRepository sessionRepository, SessionService sessionService) : base(twoFactorAuthRepository)
        {
            _twoFactorAuthRepository = twoFactorAuthRepository;
            _emailActionService = emailActionService;
            _sessionRepository = sessionRepository;
            _sessionService = sessionService;
            _userRepository = userRepository;
        }

        public bool Any2FACorrect(string code, DB_User user)
        {

            foreach (var factor in user._2FAEntries.Where(f => f.isActive))
            {

                if (_2FA.Verify(factor._2faSecret ?? string.Empty, code))
                    return true;
                else
                    continue;
            }

            return false;

        }

        public async Task<_2FAEntrySetupDTO> CreateAsync(string code, string name, DB_User user, bool ignoreCode = false)
        {

            if (ignoreCode || await _emailActionService.EmailCode_Verify(code.ToString(), user) || Any2FACorrect(code, user))
            {

                var twoFactorAuth = new DB_2FAEntry();
                twoFactorAuth.name = name;
                twoFactorAuth.userId = user.Id;
                twoFactorAuth.isActive = false;
                twoFactorAuth.recoveryCode = _2FA.BackupCode.Generate();
                twoFactorAuth._2faSecret = _2FA.Secret.Generate();
                twoFactorAuth.CreatedAt = DateTime.UtcNow;

                await _twoFactorAuthRepository.AddAsync(twoFactorAuth);
                return new _2FAEntrySetupDTO()
                {
                    Id = twoFactorAuth.Id.ToString(),
                    _2FASecret = twoFactorAuth._2faSecret,
                    RecoveryCode = twoFactorAuth.recoveryCode,
                    QrCodeUrl = _2FA.GetAuthURL(user, twoFactorAuth._2faSecret)
                };
            }
            else
            {
                throw new NauthException(WrResponseStatus.BadRequest);
            }
        }

        public async Task<bool> VerifySessionAsync(DB_Session session, string code)
        {
            if (session == null)
                throw new NauthException(WrResponseStatus.NotFound);

            if (Any2FACorrect(code, (await _userRepository.GetByIdAsync(session.userId))!))
            {
                session.is2FAConfirmed = true;
                await _sessionRepository.UpdateAsync(session);
                return true;
            }

            throw new NauthException(WrResponseStatus.BadRequest);
        }

        public async Task ActivateAsync(string code, long _2faId, long sessionId)
        {
            var twoFactorAuth = await _twoFactorAuthRepository.GetByIdAsync(_2faId);
            if (twoFactorAuth == null)
            {
                throw new NauthException( WrResponseStatus.BadRequest );
            }

            if (_2FA.Verify(twoFactorAuth._2faSecret, code.ToString()))
            {
                twoFactorAuth.isActive = true;
                await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session != null)
                {
                    session.is2FAConfirmed = true;
                    await _sessionRepository.UpdateAsync(session);
                    await _sessionService.RevokeAllSessions(session.userId, sessionId);
                }
                return;
            }
            else
            {
                throw new NauthException(WrResponseStatus.BadRequest);
            }
        }

        public async Task<_2FAEntrySetupDTO> DeleteWithRecoveryCodeAsync(string code, DB_User user)
        {
            var twoFactorAuth = await _twoFactorAuthRepository.DynamicQuerySingleAsync(q => q.Where(f => f.userId == user.Id && f.recoveryCode == code));

            if (twoFactorAuth == null || user == null)
            {
                throw new NauthException(WrResponseStatus.NotFound);
            }

            if (twoFactorAuth.recoveryCode == code)
            {
                await _twoFactorAuthRepository.DeleteAsync(twoFactorAuth);
                return await CreateAsync(twoFactorAuth.name, twoFactorAuth.name, user, true);
            }
            throw new NauthException(WrResponseStatus.BadRequest);
        }

        public async Task DeleteAsync(long id, string code, DB_User user)
        {
            var entity = await _twoFactorAuthRepository.GetByIdAsync(id);
            if (entity == null)
                throw new NauthException(WrResponseStatus.NotFound);

            //code can be iether email or valid otp code
            if (await _emailActionService.EmailCode_Verify(code.ToString(), user) || Any2FACorrect(code, user))
            {
                if (entity != null)
                {
                    await _twoFactorAuthRepository.DeleteAsync(entity);
                    return;
                }
                else
                    throw new NauthException(WrResponseStatus.BadRequest);
            }

            throw new NauthException(WrResponseStatus.BadRequest);

        }
    }
}
