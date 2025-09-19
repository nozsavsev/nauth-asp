using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class ServiceRepository(NauthDbContext context) : GenericRepository<DB_Service>(context)
    {
    }
}
