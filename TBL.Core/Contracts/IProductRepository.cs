using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.Core.Contracts
{
    public interface IProductRepository:IBaseRepository<Product>
    {
        void Update(Product obj);
        Task<IEnumerable<ProductWithCategoryNameVM>> GetProductWithProjectionToName();
    }
}
