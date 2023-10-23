using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data;
using System.Reflection;
using System.Security.Principal;

namespace Database
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<FileDb> Files { get; set; }
        public DbSet<UserDb> Users { get; set; }
        public DbSet<TokenDb> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //TODO: Составной ключ для промежуточной таблицы от отношения Token
            //TODO: Решить проблему отсутствия auto-increment в таблицах
            modelBuilder.UseIdentityColumns();
            new DbInitializer(modelBuilder).Seed().CreateThirdParty();
            base.OnModelCreating(modelBuilder);
        }
    }
}
