using HelpDesk.Domain;
using HelpDeskFrontend.Services;
using Microsoft.AspNetCore.Mvc;

public class TicketListModel : BasePageModel
{
    private readonly ApiClient? _api;

    public List<TicketListItemDto> Tickets { get; set; } = new();
    public List<TicketListItemDto> ArchivedTickets { get; set; } = new();

    public bool IsAdmin => HttpContext.Session.GetString("Role") == "admin";
    [BindProperty(SupportsGet = true)]
    public DateOnly? ArchiveDate { get; set; }

    public TicketListModel(ApiClient? api)
    {
        _api = api;
    }

    public async Task OnGet()
    {
        Tickets = await _api!.GetTicketListAsync();
        if (IsAdmin)
        {
            try
            {
                ArchivedTickets = await _api.GetArchivedTicketsAsync(ArchiveDate);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ArchivedTickets = new();
            }
        }
    }
}