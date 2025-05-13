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
        IBaseRepository<ApplicationUser> User { get; }
        IBaseRepository<ShoppingCart> ShoppingCart { get; }
        IBaseRepository<Company> Company { get; }
        Task CompleteAsync();
     
    }
}
