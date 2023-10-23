using Database;
using Database.Entities;

namespace FileAPI.Repositories.User
{
    public class UserRepository : IntRepository<UserDb>, IUserRepository
    {
        public UserRepository(ApiDbContext dbContext) : base(dbContext)
        {

        }
        public async Task<UserDb?> Authenticate(string username, string password)
        {
            return await Get(u => u.UserName == username && u.Password == password);
        }
    }
}
