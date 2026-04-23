using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDeskFrontend.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public string Message { get; set; } = "An unexpected error occurred";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet(string? message)
    {
        
        if (!string.IsNullOrWhiteSpace(message))
            Message = message;
    }
}

