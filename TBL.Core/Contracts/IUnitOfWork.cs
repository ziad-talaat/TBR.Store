using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.Contracts
{
    public interface IUnitOfWork:IDisposable
    {
       ICategory Category { get; }
        IProductRepository Products { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IVoting Vote { get; }
        IBaseRepository<ApplicationUser> User { get; }
        IBaseRepository<OrderDetails> OrderDetails { get; }
        IBaseRepository<ShoppingCart> ShoppingCart { get; }
        IBaseRepository<Company> Company { get; }
        Task CompleteAsync();
     
    }
}
