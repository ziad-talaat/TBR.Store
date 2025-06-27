using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.ViewModel
{
    public class MyOrdersVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotalPrice { get; set; }
        public string PaymentStatus { get; set; }
        public int Count { get; set; }
       // public IEnumerable<OrderDetailsVM> OrderDetails { get; set; }

       
    }

 
}
