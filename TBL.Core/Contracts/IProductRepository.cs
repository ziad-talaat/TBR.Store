
using TBL.Core.Converter;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.Core.Contracts
{
    public interface IProductRepository:IBaseRepository<Product>
    {
        void Update(Product obj);
        List<string> GetSearchValue(string value);
        List<CartItemsDetails> GetCartData(string userId);
        public int FeedBacksCount(int id);
        Task<IEnumerable<ProductWithCategoryNameVM>> GetProductWithCategoryName();
        Task<ProductWithCategoryNameVM> GetProductWithCategoryName(int id);
        CartItemsDetails? GetCartDataPerProduct(int productId, string userId);
        Pagination<Product> GetAllSortedAndFilterdInPage( string filterValue, string? value, string? sortBy, bool isAssending = true, int page = 1, string[]? includes=null);
    }
}
