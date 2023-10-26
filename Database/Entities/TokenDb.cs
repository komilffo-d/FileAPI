using Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{

    [PrimaryKey(nameof(Id), nameof(TokenName))]
    [Table("token")]
    public class TokenDb : IIntEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Column("accountid")]
        public int AccountId { get; set; }


        [Column("token_name")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TokenName { get; set; }
        [Column("timestamp")]
        public DateTime timeStamp { get; set; } = DateTime.UtcNow.AddDays(1);

        [Column("used")]
        public bool Used { get; set; } = false;
        [Column("files")]
        public List<FileDb> Files { get; set; } = new();




        [Column("account")]
        public AccountDb Account { get; set; } = null!;

    }
}
