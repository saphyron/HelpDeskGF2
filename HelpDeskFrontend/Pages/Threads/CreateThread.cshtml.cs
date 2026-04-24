using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using HelpDesk.Domain;
using HelpDeskFrontend.Services;

namespace HelpDeskFrontend.Pages;

public class CreateThreadModel : BasePageModel
{
    private readonly ApiClient _api;

    public CreateThreadModel(ApiClient api)
    {
        _api = api;
    }

    public bool IsAnonymous { get; private set; }

    [BindProperty]
    public string Title { get; set; } = "";

    [BindProperty]
    public string Body { get; set; } = "";

    [BindProperty]
    public string? AnonymousName { get; set; }

    public void OnGet()
    {
        IsAnonymous = HttpContext.Session.GetInt32("UserId") == null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        var dto = new CreateThreadDto
        {
            Title = Title,
            ThreadBody = Body,
            CreatedByUserId = userId,
            AnonymousName = userId == null ? AnonymousName : null
        };

        if (dto.CreatedByUserId == null &&
            string.IsNullOrWhiteSpace(dto.AnonymousName))
        {
            ModelState.AddModelError(string.Empty, "You must enter a name");
            IsAnonymous = true;
            return Page();
        }

        await _api.CreateThread(dto);

        return RedirectToPage("/Threads/Forum");
    }
}
