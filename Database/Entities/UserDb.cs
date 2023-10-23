using Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Database.Entities
{
    [Table("users")]
    public class UserDb: IIntEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; init; }
        [Required]
        [Column("username")]
        public string UserName { get; set; } = null!;
        [Required]
        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("tokens")]
        public List<TokenDb>? Tokens { get; set; } = new();

        [Column("files")]
        public List<FileDb>? Files { get; set; } = new();
    }
}
