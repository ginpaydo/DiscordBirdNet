using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBirdNet.Model
{
    // とりっっち使用者の情報保存
    [Table("Characters")]
    public class Character
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Like { get; set; }

        public Character()
        {
        }
    }
}
