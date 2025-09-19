using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class EmailActionRepository(NauthDbContext context) : GenericRepository<DB_EmailAction>(context)
    {
    }
}
