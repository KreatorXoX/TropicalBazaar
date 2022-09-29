using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TropicalBazaar.Models.ViewModels
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }


        public string AppuserId { get; set; }

        [ForeignKey("AppuserId")]
        [ValidateNever]
        public Appuser Appuser { get; set; }

        [NotMapped]
        public double finalPrice { get; set; }
        // cuz we dont want to save this dynamic price added to the db.

    }
}
