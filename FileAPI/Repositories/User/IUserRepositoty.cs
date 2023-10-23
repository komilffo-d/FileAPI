using Database.Entities;
using Database.Interfaces;

namespace FileAPI.Repositories.User
{
    public interface IUserRepository : IIntRepository<UserDb>
    {
        public Task<UserDb?> Authenticate(string username, string password);
    }
}
