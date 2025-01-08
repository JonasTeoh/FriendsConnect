using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpContext.Session.GetString("UserId") == null)
        {
            context.Result = RedirectToAction("Login", "User");
        }

        base.OnActionExecuting(context);
    }
}