using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Contracts
{
    public interface IUnitOfWork:IDisposable
    {
       ICategory Category { get; }
        IProductRepository Products { get; }
        Task CompleteAsync();
     
    }
}
