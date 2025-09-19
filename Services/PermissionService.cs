using Microsoft.EntityFrameworkCore;
using nauth_asp.Exceptions;
using nauth_asp.Helpers;
using nauth_asp.Models;
using nauth_asp.Repositories;
using System.Text.Json.Serialization;

namespace nauth_asp.Services
{
    public enum NauthPermissions
    {
        [JsonPropertyName("PrManageUsers")]
        PrManageUsers = 0,

        [JsonPropertyName("PrManageOwnServices")] //allows user to create and manage services
        PrManageOwnServices,

        [JsonPropertyName("PrManageServices")] //allows access to global service managment console
        PrManageServices,

        [JsonPropertyName("PrManageEmailTemplates")]
        PrManageEmailTemplates,

    }

    public class PermissionService(PermissionRepository permissionRepository) : GenericService<DB_Permission>(permissionRepository)
    {
        public async Task<List<DB_Permission>> GetUserPermissionsAsync(long userId)
        {
            return await permissionRepository.GetUserPermissionsAsync(userId);
        }

        public async Task InjectPermissions()
        {
            var permissions = Enum.GetNames<NauthPermissions>()
                .Select(p => new DB_Permission
                {
                    name = p.SplitPascalCase(),
                    key = p,
                    ServiceId = 0
                }).ToList();

            var existingPermissions = await permissionRepository.DynamicQueryManyAsync(q => q.Where(p => p.ServiceId == 0).Where(p => permissions.Select(x => x.key).Contains(p.key)));
            var newPermissions = permissions.Where(p => !existingPermissions.Any(x => x.key == p.key)).ToList();

            if (newPermissions.Any())
            {
                await permissionRepository.AddManyAsync(newPermissions);
            }
        }

        public async Task<DB_Permission> CreatePermission(CreatePermissionDTO permissionDTO)
        {
            var permission = new DB_Permission
            {
                name = permissionDTO.Name,
                key = permissionDTO.Key,
                ServiceId = long.Parse(permissionDTO.ServiceId)
            };

            permission = await permissionRepository.AddAsync(permission);

            if (permission == null)
                throw new NauthException(WrResponseStatus.BadRequest);

            return permission;
        }

        internal async Task<List<DB_Permission>> GetAll()
        {

          return await _repository.DynamicQueryManyAsync(q => q.Include(p => p.Service), true);

        }
    }
}
