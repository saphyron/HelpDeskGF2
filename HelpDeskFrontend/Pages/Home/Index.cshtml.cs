using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;

    public IndexModel(ApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public LoginRequest LoginRequest { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Hvis allerede logget ind → skip login
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            Response.Redirect("/Home/Homepage");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var result = await _api.LoginAsync(LoginRequest);

        if (result?.User == null)
        {
            ErrorMessage = "Invalid username or password";
            return Page();
        }
        
        HttpContext.Session.SetInt32("UserId", result.User.UserId);
        HttpContext.Session.SetString("Username", result.User.Username);
        HttpContext.Session.SetString("Role", result.User.Role);


        return Redirect("/Home/Homepage");
    }

    
    public async Task<IActionResult> OnPostGuest()
    {
        await _api.LoginAsGuestAsync();
        return Redirect("/Home/Homepage");
    }

}