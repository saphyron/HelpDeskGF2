using Microsoft.AspNetCore.Mvc;

public class AdminTicketsModel : BasePageModel
{
    public void OnGet()
    {

    }
        public IActionResult OnPost()
    {
        return RedirectToPage("/Admin/AdminSettings");
    }
}
