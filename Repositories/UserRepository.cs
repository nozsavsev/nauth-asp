using Microsoft.EntityFrameworkCore;
using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class UserRepository(NauthDbContext context) : GenericRepository<DB_User>(context)
    {
        public async Task<DB_User?> GetByEmailAsync(string email, bool loadDependencies = true)
        {
            return await DynamicQuerySingleAsync(q => q.Where(u => u.email == email), loadDependencies);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.email == email);
        }

        public async Task<List<long>> getAllIDs()
        {
            return await _dbSet.Select(u => u.Id).ToListAsync();
        }
    }
}
