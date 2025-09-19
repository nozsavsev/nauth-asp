using nauth_asp.DbContexts;
using nauth_asp.Models;

namespace nauth_asp.Repositories
{
    public class EmailTemplateRepository(NauthDbContext db) : GenericRepository<DB_EmailTemplate>(db)
    {
        public async Task<DB_EmailTemplate?> MostRelevantByTypeAsync(EmailTemplateType type)
        {
            return await DynamicQuerySingleAsync(t => t.Where(t => t.Type == type).OrderByDescending(t => t.isActive));
        }
    }
}
