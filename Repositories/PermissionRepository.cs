using Microsoft.EntityFrameworkCore;
using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class PermissionRepository(NauthDbContext context) : GenericRepository<DB_Permission>(context)
    {
        public async Task<List<DB_Permission>> GetUserPermissionsAsync(long userId)
        {
            var userPermissions = await _context.UserPermissions
                .Where(up => up.userId == userId)
                .Select(up => up.permission)
                .ToListAsync();
            return userPermissions ?? new();
        }

        public async Task<DB_Permission?> GetByKeyAsync(string key)
        {
            return await DynamicQuerySingleAsync(q => q.Where(p => p.key == key));
        }

        public async Task<List<DB_Permission>> GetByServiceIdAsync(long serviceId)
        {
            return await DynamicQueryManyAsync(q => q.Where(p => p.ServiceId == serviceId));
        }

        public async Task<bool> InjectUserRelations(List<DB_UserPermission> perms)
        {
            try
            {
                await _context.UserPermissions.AddRangeAsync(perms);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsByKeyAsync(string key)
        {
            return await GetByKeyAsync(key) != null;
        }
    }
}
