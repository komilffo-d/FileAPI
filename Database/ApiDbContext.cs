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
            Database.EnsureCreated();
        }

        public DbSet<FileDb> Files { get; set; }
        public DbSet<AccountDb> Users { get; set; }
        public DbSet<TokenDb> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseIdentityColumns();
            new DbInitializer(modelBuilder).Seed().CreateThirdParty();
            base.OnModelCreating(modelBuilder);
        }
    }
}
