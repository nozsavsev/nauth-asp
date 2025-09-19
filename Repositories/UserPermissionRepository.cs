using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class UserPermissionRepository(NauthDbContext context) : GenericRepository<DB_UserPermission>(context)
    {
    }
}
