using nauth_asp.Models;
using nauth_asp.Repositories;

namespace nauth_asp.Services
{
    public class UserPermissionService : GenericService<DB_UserPermission>
    {
        public UserPermissionService(UserPermissionRepository repository) : base(repository)
        {
        }
    }
}
