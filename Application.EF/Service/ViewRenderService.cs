using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TBL.Core.Contracts.ServiceContracts;

namespace TBL.EF.Service
{
    public class ViewRenderService //: IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

    //    public static async Task<string> RenderPartialViewToStringAsync(
    //Controller controller,
    //string viewName,
    //object model)
    //    {
    //        controller.ViewData.Model = model;

    //        using var writer = new StringWriter();

    //        var viewEngine = controller.HttpContext.RequestServices
    //            .GetService<ICompositeViewEngine>();

    //        var viewResult = viewEngine.FindView(
    //            controller.ControllerContext,
    //            viewName,
    //            false
    //        );

    //        if (!viewResult.Success)
    //        {
    //            throw new Exception($"View '{viewName}' not found.");
    //        }

    //        var viewContext = new ViewContext(
    //            controller.ControllerContext,
    //            viewResult.View,
    //            controller.ViewData,
    //            controller.TempData,
    //            writer,
    //            new HtmlHelperOptions()
    //        );

    //        await viewResult.View.RenderAsync(viewContext);

    //        return writer.ToString();
    //    }
    }
}
