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
    public class OrderDetails
    {
        public int Id {get; set; }
        public int OrderHeaderId { get; set; }
        [ForeignKey(nameof(OrderDetails.OrderHeaderId))]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(OrderDetails.ProductId))]
        [ValidateNever]
        public Product Product { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }

    }
}
