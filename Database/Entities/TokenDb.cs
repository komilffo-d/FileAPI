using Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    //TODO: Сделать реализацию составного ключа для сущности Token (Primary Key и Order)
    /*    [PrimaryKey(nameof(Id), nameof(TokenName))]*/
    [Table("tokens")]
    public class TokenDb : IIntEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Column("token_name")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TokenName { get; set; }
        [Column("timestamp")]
        public DateTime timeStamp { get; set; } = DateTime.UtcNow.AddDays(7);

        [Column("used")]
        public bool Used { get; set; } = false;
        [Column("files")]
        public List<FileDb>? Files { get; set; } = new List<FileDb>();


        [Column("userid")]
        public int? UserId { get; set; }
        [Column("user")]
        public UserDb? User { get; set; }

    }
}
