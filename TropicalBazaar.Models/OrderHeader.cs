using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TropicalBazaar.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        // to see who we are sending the order.
        public string AppuserId { get; set; }
        [ForeignKey("AppuserId")]
        [ValidateNever]
        public Appuser Appuser { get; set; }

        // to see where we are sending the order.
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }


        //When shipped. Carrier Details
        public DateTime ShippingDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }



        //Order related properties
        [Required]
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public string? OrderStatus { get; set; }

        //Payment related properties
        public string? PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; } // for companys who are able to pay within 30 days after the purchase

        //stripe settings for payment
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }






    }
}
