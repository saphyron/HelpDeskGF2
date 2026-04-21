
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class ForumModel : BasePageModel
{
    private ApiClient _api;
    public List<ThreadSummary> Threads { get; set; } = new();

    public ForumModel(ApiClient api) => _api = api;

    public IActionResult OnGet()
    {
        var result = _api.GetThreadsSafe().Result;

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("/Error");
        }

        Threads = result.Threads;
        return Page();
    }
}