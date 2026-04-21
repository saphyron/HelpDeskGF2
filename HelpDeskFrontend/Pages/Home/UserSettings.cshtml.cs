using Microsoft.AspNetCore.Mvc.RazorPages;

public class UserSettingsModel : BasePageModel
{
    public void OnGet()
    {
        if (!RequireLogin()) return;
    }
}