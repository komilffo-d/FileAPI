using Database.Entities;
using Database.Interfaces;

namespace FileAPI.Repositories.Account
{
    public interface IAccountRepository : IIntRepository<AccountDb>
    {
        public Task<AccountDb?> Authenticate(string login, string password);
    }
}
