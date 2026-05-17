using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Application.EF.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using TBL.Core.Contracts;
using TBL.Core.Converter;
using TBL.Core.Enums;
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
                //ImageURL=x.ImageURL,
                CategoryName = x.Category != null ? x.Category.Name : "no Category!!"
            }).FirstOrDefaultAsync(x => x.Id == id);

        }




        public void Update(Product obj)
        {
            _context.Product.Update(obj);
        }


        public List<string> GetSearchValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return new List<string>();
            return _context.Product
                .Where(x => x.Title.ToLower().StartsWith(value.ToLower()))
                .OrderByDescending(x => x.ClickedCount)
                .Take(5).Select(x=>x.Title)
                .ToList();
        }
     

     public Pagination<Product> GetAllSortedAndFilterdInPage(string filterValue,  string? sortBy, string? value, bool isAssending=true,int page = 1, string[]?includes=null)
     {
            IQueryable<Product> query = _context.Product.AsNoTracking().AsQueryable();

            if (includes != null)
            {
                foreach(var include in includes) 
                    query=query.Include(include);
            }
            if (!string.IsNullOrEmpty(filterValue))
            {
                query=BuildFilterQuery(query,filterValue);
            }

            if (!string.IsNullOrEmpty(sortBy) )
            {
                query =BuildSortQuery(query,sortBy,isAssending);
            }
            if (!string.IsNullOrEmpty(value))
            {
                query = query.Include(nameof(Product.Category));
                 query=ApplyFilterByCategory(query,value);
            }

          Pagination<Product>pageDetails= Pagination<Product>.GetPage(query, page, 8);
            return pageDetails;
     }



      public  int FeedBacksCount(int id) => _context.FeedBacks.Where(x => x.ProductId == id).Count();



        

        private IQueryable<Product> BuildSortQuery(IQueryable<Product>query,string sortBy,bool isAssending = true)
        {
            if (sortBy == "TopRated")
            {
                return isAssending
                    ? query.OrderBy(p => p.UserProduct_Voting.Count(v => v.VoteType == Voting.UpVote) -
                                         p.UserProduct_Voting.Count(v => v.VoteType == Voting.DownVote))
                    : query.OrderByDescending(p => p.UserProduct_Voting.Count(v => v.VoteType == Voting.UpVote) -
                                                   p.UserProduct_Voting.Count(v => v.VoteType == Voting.DownVote));
            }

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

        private IQueryable<Product> BuildFilterQuery(IQueryable<Product> query,string filterValue)
      {
            //var parameter = Expression.Parameter(typeof(Product), "x");
            //var property = Expression.Property(parameter, filterBy);

            //var propertyType = property.Type;
            //var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            //object? convertedValue;
            //try
            //{
            //    convertedValue = Convert.ChangeType(filterValue, targetType);
            //}
            //catch
            //{
            //    return query;
            //}

            //Expression predicate;

            //if (property.Type == typeof(string))
            //{
            //    var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
            //    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            //    var contains = Expression.Call(property, method, Expression.Constant(filterValue));
            //    predicate = Expression.AndAlso(notNull, contains);
            //}
            //else
            //{
            //    var constant = Expression.Constant(convertedValue, property.Type);
            //    predicate = Expression.Equal(property, constant);
            //}

            //var lambda = Expression.Lambda<Func<Product, bool>>(predicate, parameter);
            //return query.Where(lambda);


            if (!string.IsNullOrEmpty(filterValue))
            {
                //public string Title { get; set; }
                //public string Describtion { get; set; }
                //public string ISBN { get; set; }
                //public string Author { get; set; }
                //public double DisplayPrice { get; set; }
                string searchedValue = filterValue.ToLower().Trim();
                query = query.Where(
                     x => x.Title != null && x.Title.ToLower().Contains(searchedValue)||
                          x.Describtion != null && x.Describtion.ToLower().Contains(searchedValue)||
                          x.Author != null && x.Author.ToLower().Contains(searchedValue)||
                          x.ISBN != null && x.ISBN.ToLower().Contains(searchedValue)||
                          x.DisplayPrice.ToString()==searchedValue
                    );
            }
            return query;
        }

        private IQueryable<Product>ApplyFilterByCategory(IQueryable<Product> query, string value)
        {
            if (value == "all")
            {
                return query.AsQueryable();
            }

            var parameter = Expression.Parameter(typeof(Product), "x");
            var categoryProperty = Expression.Property(parameter, nameof(Product.Category));
            var nameProperty = Expression.Property(categoryProperty, nameof(Category.Name));

            var constant=Expression.Constant(value, nameProperty.Type);
            var equal=Expression.Equal(nameProperty, constant);
           
          

            var lambda = Expression.Lambda<Func<Product, bool>>(equal, parameter);  
            return query.Where(lambda);

        }


        public List<CartItemsDetails> GetCartData(string userId)
        {
            var data=_context.ShoppingCart.AsNoTracking()
                .Include(x=>x.User)
                .Include(x=>x.Product).ThenInclude(x=>x.ProductImages)
                .Where(x=>x.UserId==userId).Select(x=>new CartItemsDetails
                {
                    ItemName=x.Product.Title,
                    AuthorName=x.Product.Author,
                    ItemImagePath=x.Product.ProductImages.FirstOrDefault()!.ImageIrl,
                    Price= GetPriceBasedOnQuantity(x.Count,x.Product)*x.Count,
                    count=x.Count,
                    productId=x.ProductId
                }).ToList();
            return data;

        }

        public CartItemsDetails? GetCartDataPerProduct(int productId,string  userId)
        {
            var data = _context.ShoppingCart.AsNoTracking()
                .Include(x => x.Product).ThenInclude(x => x.ProductImages)
                .Where(x => x.ProductId == productId && x.UserId==userId).Select(x => new CartItemsDetails
                {
                    ItemName = x.Product.Title,
                    AuthorName = x.Product.Author,
                    ItemImagePath = x.Product.ProductImages.FirstOrDefault()!.ImageIrl,
                    Price = GetPriceBasedOnQuantity(x.Count, x.Product) * x.Count,
                    count=x.Count,
                    productId=x.ProductId

                }).FirstOrDefault();
            return data;

        }

        public static double GetPriceBasedOnQuantity(int count,Product product)
        {
            if (count <= 50)
                return product.Price;
            else if (count >= 51 && count <= 100)
                return product.Price50;
            return product.Price100;
        }
    }
}
