using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBirdNet.Model
{
    // とりっっちが購入する施設
    [Table("Facility")]
    public class Facility
    {
        // ID
        [Key]
        public int Id { get; set; }
        // 名前
        [Required]
        public string Name { get; set; }
        // 基本価格
        [Required]
        public int BasePrice { get; set; }
        // 基本利益
        [Required]
        public int BaseIncome { get; set; }
        // レベル
        [Required]
        public int Level { get; set; }
        // 現在価格
        [Required]
        public int CurrentPrice { get; set; }
        // 現在の生産量
        [Required]
        public int CurrentIncome { get; set; }
        // 説明
        public string Comment { get; set; }

        public Facility()
        {
        }
    }
}
