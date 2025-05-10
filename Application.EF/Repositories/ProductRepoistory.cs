using Application.EF.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq.Expressions;
using System.Reflection;
using TBL.Core.Contracts;
using TBL.Core.Converter;
using TBL.Core.Models;
using TBL.Core.ViewModel;


namespace TBL.EF.Repositories
{
    public class ProductRepoistory : BaseRepository<Product>, IProductRepository
    {
        public ProductRepoistory(AppDbContext context):base(context)
        {
            
        }

        public async Task<IEnumerable<ProductWithCategoryNameVM>> GetProductWithCategoryName()
        {

            return await _context.Product.AsNoTracking().Select(x => new ProductWithCategoryNameVM()
            {
                Id=x.Id,
                Title = x.Title,
                Describtion = x.Describtion,
                ISBN = x.ISBN,
                Author = x.Author,
                DisplayPrice = x.DisplayPrice,
                Price = x.Price,
                Price50 = x.Price50,
                Price100 = x.Price100,
               CategoryName = x.Category != null ? x.Category.Name : "no Category!!"
            }).ToListAsync();

        }

        public async Task<ProductWithCategoryNameVM> GetProductWithCategoryName(int id)
        {

            return await _context.Product.AsNoTracking().Select(x => new ProductWithCategoryNameVM()
            {
                Id = x.Id,
                Title = x.Title,
                Describtion = x.Describtion,
                ISBN = x.ISBN,
                Author = x.Author,
                DisplayPrice = x.DisplayPrice,
                Price = x.Price,
                Price50 = x.Price50,
                Price100 = x.Price100,
                ImageURL=x.ImageURL,
                CategoryName = x.Category != null ? x.Category.Name : "no Category!!"
            }).FirstOrDefaultAsync(x => x.Id == id);

        }




        public void Update(Product obj)
        {
            _context.Product.Update(obj);
        }

     

     public Pagination<Product> GetAllSortedAndFilterdInPage(string? filterBy,string filterValue,string? sortBy,bool isAssending=true,int page = 1)
     {
            IQueryable<Product> query = _context.Product.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                query=BuildFilterQuery(query,filterBy,filterValue);
            }

            if (!string.IsNullOrEmpty(sortBy) )
            {
                query=BuildSortQuery(query,sortBy,isAssending);
            }
          Pagination<Product>pageDetails= Pagination<Product>.GetPage(query, page, 8);
            return pageDetails;
     } 







        

        private IQueryable<Product> BuildSortQuery(IQueryable<Product>query,string sortBy,bool isAssending = true)
        {
            var parameter = Expression.Parameter(typeof(Product), "x");
            var property = Expression.Property(parameter, sortBy);
            var propertyType = property.Type;
            var methodName = isAssending == true ? "OrderBy" : "OrderByDescending";  
            var lambda=Expression.Lambda(property, parameter);

            var methodCall = Expression.Call(
                typeof(Queryable),
                methodName, new Type[] { typeof(Product), propertyType },
                query.Expression
                , Expression.Quote(lambda));
           return  query.Provider.CreateQuery<Product>(methodCall);
        }

        private IQueryable<Product> BuildFilterQuery(IQueryable<Product> query, string filterBy,string filterValue)
        {
            var parameter = Expression.Parameter(typeof(Product), "x");
            var property = Expression.Property(parameter, filterBy);

            var propertyType=property.Type;
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            //var convertedValue=Convert.ChangeType(filterValue,propertyType);
            object convertedValue;
            try
            {
                convertedValue = Convert.ChangeType(filterValue, targetType);
            }
            catch
            {
                // Invalid filterValue; return unfiltered query or throw depending on your needs
                return query;
            }

            var constant = Expression.Constant(convertedValue, propertyType);

           var  equal=Expression.Equal(property,constant);
            var lambda=Expression.Lambda<Func<Product,bool>>(equal, parameter);

           return  query.Where(lambda);

            
        }
    }

   
}
