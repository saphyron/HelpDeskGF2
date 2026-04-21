using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Security;

public abstract class BasePageModel : PageModel
{
    protected bool RequireLogin()
    {
        if (!SecurityFunctions.IsLoggedIn(HttpContext))
        {
            Response.Redirect("/Home");
            return false;
        }
        return true;
    }

    protected bool RequireAdmin()
    {
        if (!SecurityFunctions.IsAdmin(HttpContext))
        {
            Response.StatusCode = 403;
            return false;
        }
        return true;
    }
}