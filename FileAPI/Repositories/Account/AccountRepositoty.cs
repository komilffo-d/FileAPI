using Database;
using Database.Entities;
using FileAPI.Repositories.Account;

namespace FileAPI.Repositories.Account
{
    public class AccountRepository : IntRepository<AccountDb>, IAccountRepository
    {
        public AccountRepository(ApiDbContext dbContext) : base(dbContext)
        {

        }
        public async Task<AccountDb?> Authenticate(string login, string password)
        {
            return await Get(u => u.Login == login && u.Password == password);
        }
    }
}
