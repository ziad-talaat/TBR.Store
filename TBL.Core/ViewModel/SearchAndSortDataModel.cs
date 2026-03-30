using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.ViewModel
{
    public class SearchAndSortDataModel
    {
        public string? searchValue { get; set; }
        public string? sortBy { get; set; }
        public string? categoryValue { get; set; }

        public bool isAssending { get; set; } = true;

        public int pageNumber { get; set; }=1;
        public bool? fromSearch { get; set; } = false;
    }
}
