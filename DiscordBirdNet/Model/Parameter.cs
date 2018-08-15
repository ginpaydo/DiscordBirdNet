using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBirdNet.Model
{
    // とりっっちのパラメータ保存
    [Table("Parameters")]
    public class Parameter
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }

        public Parameter()
        {
        }
    }
}
