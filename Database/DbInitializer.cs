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

        _modelBuilder.Entity<FileDb>().HasData(new FileDb[]
        {
                            new FileDb{
                                Id=1,
                                FileName=@"Files\123.pdf",
                                FileType=FileType.PDF
                            },
                            new FileDb{
                                Id=2,
                                FileName=@"Files\456.docx",
                                FileType=FileType.DOCX
                            }
        });
        _modelBuilder.Entity<TokenDb>().HasData(new TokenDb[]
        {
                            new TokenDb{
                                Id=1,
                                TokenName=Guid.NewGuid(),

                        },
                            new TokenDb{
                                Id=2,
                                TokenName=Guid.NewGuid()
                            }
        });
        _modelBuilder.Entity<UserDb>().HasData(new UserDb[]
            {
                            new UserDb{
                                Id=1,
                                UserName="admin",
                                Password="admin"
                            },
                            new UserDb{
                                Id=2,
                                UserName="user",
                                Password="user"
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
            x => x.HasOne<TokenDb>().WithMany().HasForeignKey("token_name"),
             x => x.HasOne<FileDb>().WithMany().HasForeignKey("file_id"),
             x => x.ToTable("filetoken"));
        return this;
    }
}