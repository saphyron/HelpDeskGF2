
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class ForumModel : PageModel
{
    private ApiClient _api;
    public List<ThreadSummary> Threads { get; set; }

    public ForumModel(ApiClient api) => _api = api;

    public async Task OnGet()
    {
        Threads = await _api.GetThreads();
    }
}