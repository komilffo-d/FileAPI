using Database;
using Database.Entities;

namespace FileAPI.Repositories.Token
{
    public class TokenRepository : IntRepository<TokenDb>
    {
        public TokenRepository(ApiDbContext dbContext) : base(dbContext)
        {

        }
    }
}
