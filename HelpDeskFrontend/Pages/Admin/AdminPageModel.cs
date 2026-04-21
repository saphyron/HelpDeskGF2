using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

public abstract class AdminPageModel : PageModel
{
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var role = HttpContext.Session.GetString("Role");

        if (role != "admin")
        {
            // Stopper AL adgang for ikke-admin
            context.Result = new RedirectToPageResult("/Home/Homepage");
            return;
        }

        base.OnPageHandlerExecuting(context);
    }
}
