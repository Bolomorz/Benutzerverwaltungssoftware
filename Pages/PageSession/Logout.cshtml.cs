using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Benutzerverwaltungssoftware.Pages.PageSession;

public class LogoutModel : PageModel
{
    public LogoutModel(){}

    public IActionResult OnGet()
    {
        Global.CloseSession();
        return RedirectToPage("/Index");
    }
}