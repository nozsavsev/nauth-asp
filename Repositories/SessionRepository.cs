using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class SessionRepository(NauthDbContext context) : GenericRepository<DB_Session>(context)
    {
        public async Task<List<DB_Session>> GetByUserIdAsync(long userId)
        {
            return await DynamicQueryManyAsync(q => q.Where(s => s.userId == userId));
        }

        public async Task<List<DB_Session>> GetByServiceIdAsync(long serviceId)
        {
            return await DynamicQueryManyAsync(q => q.Where(s => s.serviceId == serviceId));
        }

        public async Task<List<DB_Session>> GetActiveSessionsAsync()
        {
            return await DynamicQueryManyAsync(q => q.Where(s => s.ExpiresAt > DateTime.UtcNow));
        }

        public async Task<List<DB_Session>> GetExpiredSessionsAsync()
        {
            return await DynamicQueryManyAsync(q => q.Where(s => s.ExpiresAt <= DateTime.UtcNow));
        }

        public async Task<int> DeleteExpiredSessionsAsync()
        {
            var expiredSessions = await GetExpiredSessionsAsync();
            await DeleteManyAsync(expiredSessions);
            return expiredSessions.Count;
        }
    }
}
