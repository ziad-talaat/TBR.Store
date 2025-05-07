using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.Contracts
{
    public interface ICategory:IBaseRepository<Category>
    {
        void Update(Category category);
    }
}
