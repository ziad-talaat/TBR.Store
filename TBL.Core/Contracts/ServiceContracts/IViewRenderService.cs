using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TBL.Core.Contracts.ServiceContracts
{
    public interface IViewRenderService
    {
         Task<string> RenderPartialViewToStringAsync(
    Controller controller,
    string viewName,
    object model);
    }
}
