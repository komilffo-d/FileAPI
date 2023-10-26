using Database.Entities;
using Database.Enums;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class DbInitializer : IDbInitializer<ModelBuilder>
{
    private readonly ModelBuilder _modelBuilder;

    public DbInitializer(ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }

    public IDbInitializer<ModelBuilder> Seed()
    {

        _modelBuilder.Entity<AccountDb>().HasData(new AccountDb[]
            {
                            new AccountDb{
                                Id=1,
                                Login="admin",
                                Password="admin",
                                Role=Role.ADMIN
                            },
                            new AccountDb{
                                Id=2,
                                Login="user",
                                Password="user",
                                Role=Role.USER
                            }
            });
        return this;
    }

    public IDbInitializer<ModelBuilder> CreateThirdParty()
    {

        _modelBuilder.Entity<FileDb>()
            .HasMany(f => f.Tokens)
            .WithMany(t => t.Files)
            .UsingEntity<Dictionary<string, string>>("filetoken",
            x => x.HasOne<TokenDb>().WithMany().HasForeignKey("token_id","token_name"),
             x => x.HasOne<FileDb>().WithMany().HasForeignKey("file_id"),
             x => x.ToTable("filetoken"));

        return this;
    }
}