using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TropicalBazaar.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 1 - 50")]
        public double Price { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 50+")]
        public double Price50 { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }




        [Display(Name = "Homegrown || Imported")]
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")] // tells below code that is gonna be used in categoryId as foreignkey.
        [ValidateNever]
        public Category Category { get; set; } //navigation property





        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [ForeignKey("UnitId")] // tells below code that is gonna be used in unitId as foreignkey.
        [ValidateNever]
        public Unit Unit { get; set; } //navigation property
    }
}
