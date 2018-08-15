using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBirdNet.Model
{
    // とりっっちが購入する商品
    [Table("Commodity")]
    class Commodity
    {
        // ID
        [Key]
        public int Id { get; set; }
        // 名前
        [Required]
        public string Name { get; set; }
        // 価格
        [Required]
        public int Price { get; set; }
        // 効果の種類
        [Required]
        public int EffectKind { get; set; }
        // 効果の大きさ
        [Required]
        public int EffectValue { get; set; }
        // 効果の対象
        [Required]
        public int EffectObject { get; set; }
        // 説明
        public string Comment { get; set; }

        public Commodity()
        {
        }
    }
}
// Facility