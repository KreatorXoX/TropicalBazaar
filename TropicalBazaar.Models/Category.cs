using System.ComponentModel.DataAnnotations;

namespace TropicalBazaar.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        /* format for entering name is (Imported From "3letterCountry name") or (Homegrown) */
        public string Name { get; set; }

        [Display(Name = "Amount")]
        [Range(1, 500, ErrorMessage = "Amount should be between 1 - 500")]
        public int Quantity { get; set; }

    }
}
