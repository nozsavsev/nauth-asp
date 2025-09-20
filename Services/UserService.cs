using Google.Apis.Auth;
using Microsoft.AspNetCore.SignalR;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using nauth_asp.Services.ObjectStorage;
using nauth_asp.SignalRHubs;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace nauth_asp.Services
{
    public class UserService : GenericService<DB_User>
    {

        private readonly Regex passwordRegex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d!@#$%^&*()_\-+=\[{\]};:'"",<.>/?]{6,32}$");
        private readonly UserRepository userRepository;
        private readonly SessionService sessionService;
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<AuthHub> _hubContext;
        private readonly IUserRefreshService _userRefreshService;
        private readonly IObjectStorageService _objectStorageService;
        private readonly string _avatarBucketName;
        private readonly UserPermissionRepository _userPermissionRepository;
        private readonly IConfiguration _config;

        public UserService(UserRepository _userRepository, UserPermissionRepository userPermissionRepository, SessionService _sessionService, IServiceProvider _serviceProvider, IHubContext<AuthHub> hubContext, IObjectStorageService objectStorageService, IConfiguration config, IUserRefreshService userRefreshService) : base(_userRepository)
        {
            userRepository = _userRepository;
            sessionService = _sessionService;
            serviceProvider = _serviceProvider;
            _hubContext = hubContext;
            _objectStorageService = objectStorageService;
            _config = config;
            _avatarBucketName = config["Amazon:avatarBucketName"]!;
            _userPermissionRepository = userPermissionRepository;
            _userRefreshService = userRefreshService;
        }

        public class UserAndToken
        {
            public DB_User user { get; set; } = null!;
            public string token { get; set; } = null!;
        }

        public async Task<bool> SetUserPassword(long userId, string password, long? ignoreSessionId = null)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) return false;


            if (!passwordRegex.IsMatch(password))
                return false;

            user.passwordHash = Argon2idHasher.Hash(password);
            await userRepository.UpdateAsync(user);
            await sessionService.RevokeAllSessions(userId, ignoreSessionId);
            _userRefreshService.QueueUserRefresh(userId);
            return true;
        }

        public async Task<UserAndToken> RegisterWithEmailAndPasswordAsync(CreateUserDTO userCreateDTO)
        {

            if (await ExistsByEmailAsync(userCreateDTO.Email))
                throw new NauthException(WrResponseStatus.EmailNotAvailable);

            if (userCreateDTO.Email.Length > 128 || (new MailAddress(userCreateDTO.Email).Address != userCreateDTO.Email))
                throw new NauthException(WrResponseStatus.InvalidEmail);

            if (!passwordRegex.IsMatch(userCreateDTO.Password))
                throw new NauthException(WrResponseStatus.InvalidPassword);


            var user = new DB_User();

            user.email = userCreateDTO.Email;
            user.passwordHash = Argon2idHasher.Hash(userCreateDTO.Password);

            user.isEmailVerified = false;

            user = await userRepository.AddAsync(user);

            if (user == null) throw new NauthException(WrResponseStatus.InternalError);

            if (user.isEnabled == false) throw new NauthException(WrResponseStatus.RequireEnabledUser);

            var token = await sessionService.IssueSession(user.Id);

            if (token == null) throw new NauthException(WrResponseStatus.InternalError);

            _userRefreshService.QueueUserRefresh(user.Id);

            return new UserAndToken
            {
                user = (await userRepository.GetByIdAsync(user.Id))!, // to load new session
                token = token
            };
        }

        public async Task<UserAndToken> LoginWithEmailAndPasswordAsync(CreateUserDTO userCreateDTO)
        {

            var passwordRegex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d!@#$%^&*()_\-+=\[{\]};:'"",<.>/?]{6,32}$");

            if (userCreateDTO.Email.Length > 128 || (new MailAddress(userCreateDTO.Email).Address != userCreateDTO.Email))
                throw new NauthException(WrResponseStatus.InvalidEmail);

            var user = await userRepository.GetByEmailAsync(userCreateDTO.Email, loadDependencies: false);

            if (user == null)
                throw new NauthException(WrResponseStatus.NotFound);

            if (user.isEnabled == false)
                throw new NauthException(WrResponseStatus.RequireEnabledUser);


            if (!Argon2idHasher.Verify(userCreateDTO.Password, user.passwordHash))
                throw new NauthException(WrResponseStatus.InvalidPassword);

            var token = await sessionService.IssueSession(user.Id);

            if (token == null) throw new NauthException(WrResponseStatus.InternalError);

            return new UserAndToken
            {
                user = (await userRepository.GetByIdAsync(user.Id))!, // to load new session
                token = token
            };
        }


        public async Task<UserAndToken> ContinueWithGoogle(string GoogleAccessToken)
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _config["Google:ClientId"]! }
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(GoogleAccessToken, validationSettings);

            string email = payload.Email.ToLower() ?? throw new NauthException(WrResponseStatus.BadRequest);
            DB_User? user = await GetByEmailAsync(email, loadDependencies: false);

            if (user == null)
            {
                //register user
                user = new DB_User();

                user.isEmailVerified = true;
                user.Name = payload.Name;
                user.email = email;

                user.AvatarURL = payload.Picture;

                user = await userRepository.AddAsync(user);
            }
            else
            {

                if (user.isEnabled == false) throw new NauthException(WrResponseStatus.RequireEnabledUser);

                bool updateUserNeeded = false;

                if (user.isEmailVerified == false)
                {
                    user.isEmailVerified = true;
                    updateUserNeeded = true;
                }
                if (user.Name == null)
                {
                    user.Name = payload.Name;
                    updateUserNeeded = true;
                }
                if (user.AvatarURL == null)
                {
                    user.AvatarURL = payload.Picture;
                    updateUserNeeded = true;
                }

                if (updateUserNeeded)
                {

                    await _repository.UpdateAsync(user);

                    _userRefreshService.QueueUserRefresh(user.Id);
                }
            }



            var token = await sessionService.IssueSession(user.Id);

            if (token == null) throw new NauthException(WrResponseStatus.InternalError);

            return new UserAndToken
            {
                user = (await userRepository.GetByIdAsync(user.Id))!, // to load new session
                token = token
            };
        }


        public async Task<DB_User?> GetByEmailAsync(string email, bool loadDependencies = true)
        {
            var user = await userRepository.GetByEmailAsync(email, loadDependencies);
            return user;
        }

        public async Task<List<DB_User>> GetAllAsync()
        {
            var users = await userRepository.DynamicQueryManyAsync();
            return users;
        }


        public async Task DeleteAsync(long id)
        {
            //TODO delete shared resources
            var user = await userRepository.GetByIdAsync(id, tracking: true, loadDependencies: false);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.AvatarURL))
                {
                    var key = KeyGenerators.GetUserAvatarKey(user.Id);
                    await _objectStorageService.DeleteFileAsync(_avatarBucketName, key);
                }
                await _hubContext.Clients.Group(id.ToString()).SendAsync("Deleted");
                await userRepository.DeleteAsync(user);
            }
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await userRepository.ExistsAsync(id);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await userRepository.ExistsByEmailAsync(email);
        }

        internal async Task VerifyEmailAsync(long id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.isEmailVerified = true;
                await userRepository.UpdateAsync(user);
                _userRefreshService.QueueUserRefresh(id);
            }
        }

        internal async Task SetUserEmailAsync(long id, string newEmail)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.email = newEmail;
                await userRepository.UpdateAsync(user);
                _userRefreshService.QueueUserRefresh(id);
            }
        }


        internal async Task<UserAndToken> LoginWithEmailAndCodeAsync(string email, string code)
        {
            var user = await userRepository.GetByEmailAsync(email, loadDependencies: false);
            if (user == null)
                throw new NauthException(WrResponseStatus.NotFound);

            if (user.isEnabled == false) throw new NauthException(WrResponseStatus.RequireEnabledUser);

            var emailActions = await serviceProvider.GetRequiredService<EmailActionService>().EmailSignIn_Verify(code, user);
            if (!emailActions)
                throw new NauthException(WrResponseStatus.BadRequest);

            var token = await sessionService.IssueSession(user.Id);
            if (token == null) throw new NauthException(WrResponseStatus.InternalError);

            return new UserAndToken { user = user, token = token };
        }



        public async Task<DB_User> UpdateUserName(long userId, string name)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NauthException(WrResponseStatus.NotFound);

            if (name.Length > 128)
                name = name.Substring(0, 128);

            user.Name = name;
            var updatedUser = await userRepository.UpdateAsync(user);
            _userRefreshService.QueueUserRefresh(userId);
            return updatedUser;
        }

        public async Task<DB_User> SetAvatarUrlAsync(long userId, string? avatarUrl)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NauthException(WrResponseStatus.NotFound);

            user.AvatarURL = avatarUrl;
            var updatedUser = await userRepository.UpdateAsync(user);
            _userRefreshService.QueueUserRefresh(userId);
            return updatedUser;
        }

        // user manager area

        public async Task<DB_User> UpdatePermissions(long userId, List<long> permissionIds)
        {
            permissionIds = permissionIds.Distinct().ToList();

            var user = await userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null)
                throw new NauthException(WrResponseStatus.NotFound);

            user.permissions.Clear();
            if (permissionIds != null)
            {

                var toInsert = permissionIds.Select(pId => new DB_UserPermission { permissionId = pId, userId = userId }).ToList();
                await _userPermissionRepository.AddManyAsync(toInsert);

            }
            var updatedUser = await userRepository.GetByIdAsync(userId);
            _userRefreshService.QueueUserRefresh(userId);
            return updatedUser!;
        }

        internal async Task DisableUserAsync(long id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.isEnabled = false;
                await userRepository.UpdateAsync(user);
                _userRefreshService.QueueUserRefresh(id);
            }
        }

        internal async Task UnVerifyEmailAsync(long id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.isEmailVerified = false;
                await userRepository.UpdateAsync(user);
                _userRefreshService.QueueUserRefresh(id);
            }
        }

        internal async Task EnableUserAsync(long id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.isEnabled = true;
                await userRepository.UpdateAsync(user);
                _userRefreshService.QueueUserRefresh(id);
            }
        }

        internal async Task<List<DB_User>> AdminGetUsers(string? match = null, int skip = 0, int take = 20)
        {
            var users = await userRepository.DynamicQueryManyAsync(q =>
            {

                if (!string.IsNullOrEmpty(match))
                    q = q.Where(u => u.email.Contains(match) || (u.Name != null && u.Name.Contains(match)));

                q.OrderByDescending(u => u.Id);

                if (skip > 0)
                    q = q.Skip(skip);

                if (take > 0)
                    q = q.Take(take);

                return q;

            }, true);

            return users ?? new();

        }
    }
}
