
using TBL.Core.Converter;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.Core.Contracts
{
    public interface IProductRepository:IBaseRepository<Product>
    {
        void Update(Product obj);
        Task<IEnumerable<ProductWithCategoryNameVM>> GetProductWithCategoryName();
        Task<ProductWithCategoryNameVM> GetProductWithCategoryName(int id);
        Pagination<Product> GetAllSortedAndFilterdInPage(string? filterBy, string filterValue, string? value, string? sortBy, bool isAssending = true, int page = 1);
    }
}
