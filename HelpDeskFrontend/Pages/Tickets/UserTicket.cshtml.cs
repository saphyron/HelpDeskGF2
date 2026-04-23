using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;

public class UserTicketModel : BasePageModel
{
    private readonly ApiClient _api;

    public int TicketCount { get; set; }

    public UserTicketModel(ApiClient api)
    {
        _api = api;
    }

    public async Task OnGet()
    {
        TicketCount = await _api.GetNumberTicketsForTheDay();
    }

    public bool IsGuest => HttpContext.Session.GetString("Role") == "guest";

    public async Task<IActionResult> OnPost()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("Role");

        if (userId == null || role == "guest")
        {
            TempData["ErrorMessage"] = "User not logged in or user is guest";
            return RedirectToPage("/Error");
        }
       
        try
        {
            await _api.CreateTicketAsync(userId.Value, role!);
            return RedirectToPage("/Tickets/UserTicket");
        }
        catch (Exception ex)
        {
        return RedirectToPage("/Error", new { message = ex.Message });
        }


        
        
    }
}