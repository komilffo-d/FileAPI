using Database.Enums;
using Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Database.Entities
{
    [Table("files")]
    public class FileDb : IIntEntity
    {

        [Key]
        [Column("id")]
        public int Id { get; init; }
        [Required]
        [Column("file_name")]
        public string FileName { get; set; }
        [Column("file_type")]
        public FileType FileType { get; set; }
        [Column("tokens")]
        public List<TokenDb>? Tokens { get; set; } = new List<TokenDb>();
        [Column("users")]
        public List<UserDb>? Users { get; set; } = new List<UserDb>();

    }
}
