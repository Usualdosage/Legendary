// <copyright file="ContentHelper.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    /// <summary>
    /// Helps render content from Razor views.
    /// </summary>
    public static class ContentHelper
    {
        /// <summary>
        /// Render the content of a view as HTML.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="model">The model.</param>
        /// <param name="partial">Whether this is a partial view or not.</param>
        /// <returns>HTML string.</returns>
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine? viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult? viewResult = viewEngine?.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult != null)
                {
                    if (viewResult.Success == false)
                    {
                        return $"<h1>A view with the name {viewName} could not be found.</h1>";
                    }

                    ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        writer,
                        new HtmlHelperOptions());

                    await viewResult.View.RenderAsync(viewContext);

                    var stringContent = writer.GetStringBuilder().ToString();
                    stringContent = stringContent.Replace("\r\n", string.Empty);
                    var content = System.Web.HttpUtility.HtmlEncode(stringContent);
                    return content;
                }
                else
                {
                    return $"<h1>A view with the name {viewName} could not be found.</h1>";
                }
            }
        }
    }
}



