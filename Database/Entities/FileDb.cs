using Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    [Table("file")]
    public class FileDb : IIntEntity
    {

        [Key]
        [Column("id")]
        public int Id { get; init; }

        [Column("accountid")]
        public int AccountId { get; set; }

        [Required]
        [Column("file_name")]
        public string FileName { get; set; }

        [Required]
        [Column("file_type")]
        public string FileType { get; set; }

        [Required]
        [Column("shared")]
        public bool Shared { get; set; }

        [Column("tokens")]
        public List<TokenDb> Tokens { get; set; } = new();


        [Column("account")]
        public AccountDb Account { get; set; } = null!;

    }
}
