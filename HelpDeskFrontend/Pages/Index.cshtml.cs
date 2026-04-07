
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class IndexModel : PageModel
{
    private ApiClient _api;

    public IndexModel(ApiClient api) => _api = api;

    [BindProperty] public string Username { get; set; }
    [BindProperty] public string Password { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var result = await _api.Login(new LoginRequest
        {
            Username = Username,
            Password = Password
        });

        if (result == null)
        {
            Error = "Forkert login";
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", result.User.UserId);
        HttpContext.Session.SetString("Name", result.User.Name ?? "Anonym");

        return RedirectToPage("/Forum");
    }
}