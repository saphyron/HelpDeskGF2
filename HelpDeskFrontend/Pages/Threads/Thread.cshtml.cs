using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HelpDesk.Domain;
using HelpDeskFrontend.Services;

public class ThreadModel : BasePageModel
{
    private readonly ApiClient _api;

    public ThreadModel(ApiClient api)
    {
        _api = api;
    }

    public ThreadDto? CurrentThread { get; set; }

    public bool IsAnonymous { get; set; }

    [BindProperty]
    public string ResponseText { get; set; } = "";

    [BindProperty]
    public string? AnonymousName { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        CurrentThread = await _api.GetThread(id);

        if (CurrentThread == null)
            return RedirectToPage("/Threads/Forum");

        IsAnonymous = HttpContext.Session.GetString("Role") == "guest";
        return Page();
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
        return RedirectToPage("/Threads/Thread", new { id });
    }
}