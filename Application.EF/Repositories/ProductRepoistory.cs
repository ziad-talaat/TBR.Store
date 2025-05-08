using Application.EF.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Models;
using TBL.Core.ViewModel;

namespace TBL.EF.Repositories
{
    public class ProductRepoistory : BaseRepository<Product>, IProductRepository
    {
        public ProductRepoistory(AppDbContext context):base(context)
        {
            
        }

        public async Task<IEnumerable<ProductWithCategoryNameVM>> GetProductWithProjectionToName()
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
                CategoryName = x.Category.Name,
            }).ToListAsync();

        }

        public void Update(Product obj)
        {
            _context.Product.Update(obj);
        }
    }
}
