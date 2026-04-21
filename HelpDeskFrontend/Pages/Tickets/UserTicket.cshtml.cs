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

    public async Task<IActionResult> OnPost()
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        if (userId > 0)
        {
            TempData["ErrorMessage"] = "User not logged in";
            return RedirectToPage("/Error");
        }
        await _api.CreateTicketAsync();

        return RedirectToPage("/Tickets/UserTicket");
        
        
    }
}