using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TBR.Store.Areas.Customer.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //ViewBag.ReturnUrl = Request.Path + Request.QueryString;


            // Only store state for Index page
            if (context.HttpContext.Request.Path.Equals("/Customer/Home/Index", StringComparison.OrdinalIgnoreCase)
                && context.HttpContext.Request.Method == "GET")
            {
                // Save full query string including filters/sort/page
                HttpContext.Session.SetString("LastIndexUrl", context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
            }
            base.OnActionExecuting(context);
        }
    
    }
}
