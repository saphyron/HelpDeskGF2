
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class ForumModel : BasePageModel
{
    private ApiClient _api;
    public List<ThreadSummary> Threads { get; set; } = new();

    public ForumModel(ApiClient api) => _api = api;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role") ?? "guest";
        var result = await _api.GetThreadsSafe(userId, role);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToPage("/Error");
        }

        Threads = result.Threads;
        return Page();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.ToString();
            return RedirectToPage("/Error");
        }
    }
}