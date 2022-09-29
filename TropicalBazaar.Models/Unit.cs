using System.ComponentModel.DataAnnotations;

namespace TropicalBazaar.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        /* format for entering unit is kg,lb or piece /*/
        public string Name { get; set; }
    }
}
