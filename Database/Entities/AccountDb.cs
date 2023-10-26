using Database.Enums;
using Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    [Table("account")]
    public class AccountDb : IIntEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; init; }
        [Required]
        [Column("login")]
        public string Login { get; set; } = null!;
        [Required]
        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("role")]
        public Role Role { get; set; } = Role.USER;

        [Column("tokens")]
        public List<TokenDb> Tokens { get; set; } = new();

        [Column("files")]
        public List<FileDb> Files { get; set; } = new();
    }
}
