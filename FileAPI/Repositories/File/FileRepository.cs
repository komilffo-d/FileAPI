using Database;
using Database.Entities;

namespace FileAPI.Repositories.File
{
    public class FileRepository : IntRepository<FileDb>
    {
        public FileRepository(ApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
