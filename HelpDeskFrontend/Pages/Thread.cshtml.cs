
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class ThreadModel : PageModel
{
    private ApiClient _api;

    public ThreadDto Thread { get; set; }

    public ThreadModel(ApiClient api) => _api = api;

    [BindProperty] public string Response { get; set; }

    public async Task OnGet(int id)
    {
        Thread = await _api.GetThread(id);
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        await _api.AddResponse(id, new AddThreadResponseDto
        {
            ResponseBody = Response,
            CreatedByUserId = userId
        });

        return RedirectToPage("/Thread", new { id });
    }
}