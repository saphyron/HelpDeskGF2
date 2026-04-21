using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AdminSettingsModel : AdminPageModel
{
    public void OnGet()
    {

    }
    
    public IActionResult OnPost()
    {
        // TODO: reset password via backend
        return RedirectToPage("/Admin/AdminSettings");
    }

}
