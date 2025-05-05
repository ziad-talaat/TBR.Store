using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.Converter
{
    public static  class Converter
    {
        public static void  ConvertToCategoryDTO(this Category categoryToUdate,Category hasData)
        {
            categoryToUdate.Name= hasData.Name;
            categoryToUdate.DisplayOrder = hasData.DisplayOrder;
        }
    }
}
