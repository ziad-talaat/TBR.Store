using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDueDate { get; set; }
        public double OrderTotal { get; set; }
        public string ?OrderStatus { get; set; }
        public string ?PaymentStatus { get; set; }
        public string ?TrackingNumber { get; set; }
        public string ?Carrier { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; }

        public string?SessionId { get; set; }
        public string?PaymentIntentId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Adress { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Name { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(OrderHeader.UserId))]
        public ApplicationUser User { get; set; }
        

        public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

    }
}
