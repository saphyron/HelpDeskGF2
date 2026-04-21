using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class AdminThreadsModel : AdminPageModel
{
    public void OnGet()
    {

    }
    public IActionResult OnPost()
    {
        return RedirectToPage("/Admin/AdminSettings");
    }
}
