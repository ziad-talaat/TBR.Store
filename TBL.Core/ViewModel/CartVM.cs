using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.ViewModel
{
    public class CartVM
    {
        public IEnumerable<ShoppingCart> CartList {  get; set; }
        public double OrderTotal { get; set; }
    }
}
