using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;

public class ThreadModel : PageModel
{
    private readonly ApiClient _api;

    public ThreadDto Thread { get; set; } = null!;

    public ThreadModel(ApiClient api) => _api = api;

    [BindProperty] public string ResponseText { get; set; } = "";

    [BindProperty] public string? AnonymousName { get; set; }

    public async Task OnGet(int id)
    {
        Thread = await _api.GetThread(id);
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        var dto = new AddThreadResponseDto
        {
            ThreadId = id,
            ResponseBody = ResponseText,
            CreatedByUserId = userId,
            AnonymousName = userId == null ? AnonymousName : null
        };

        if (dto.CreatedByUserId == null &&
            string.IsNullOrWhiteSpace(dto.AnonymousName))
        {
            ModelState.AddModelError("", "You must enter a name");
            await OnGet(id);
            return Page();
        }

        await _api.AddResponse(id, dto);

        return RedirectToPage("/Thread", new { id });
    }
}