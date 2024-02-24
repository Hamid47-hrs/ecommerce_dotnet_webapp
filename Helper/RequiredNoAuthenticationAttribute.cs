using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ecommerce_dotnet_webapp.Helper;

public class RequiredNoAuthenticationAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        string? role = context.HttpContext.Session.GetString("role");

        if (role != null)
        {
            context.Result = new RedirectResult("/");
        }
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
}
