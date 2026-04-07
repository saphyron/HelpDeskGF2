
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;


public class CreateThreadModel : PageModel
{
    private ApiClient _api;

    public CreateThreadModel(ApiClient api) => _api = api;

    [BindProperty] public string Title { get; set; }
    [BindProperty] public string Body { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        await _api.CreateThread(new CreateThreadDto
        {
            Title = Title,
            ThreadBody = Body,
            CreatedByUserId = userId
        });

        return RedirectToPage("/Forum");
    }
}