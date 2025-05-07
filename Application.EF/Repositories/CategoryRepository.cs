using Application.EF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Models;

namespace TBL.EF.Repositories
{
    public class CategoryRepository : BaseRepository<Category>,ICategory
    {
        public CategoryRepository(AppDbContext context):base(context)
        {
            
        }
        public void Update(Category category)
        {
           _context.Category.Update(category);
        }
    }
}
