using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class _2FARepository(NauthDbContext context) : GenericRepository<DB_2FAEntry>(context)
    {



    }
}
