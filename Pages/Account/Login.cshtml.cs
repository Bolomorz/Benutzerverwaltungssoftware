using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Benutzerverwaltungssoftware.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty] public LoginDataModel LDM { get; set; }

    public LoginModel()
    {
        LDM = new(){ Username = "", Password = "" };
    }

    public void OnGet()
    {
        try
        {
            var rd = Session.Session.TestDatabaseConnection();
            Information.Message = rd.Message;
        }
        catch(Exception ex)
        {
            Information.Message = new(Data.MID.ErrorThrown, false, $"Datenbankverbindung nicht erfolgreich hergestellt.\n\n{ex}");
        }
    }

    public IActionResult OnPostLogin()
    {
        var rdv = LDM.Validate();
        if(!rdv.Message.Success)
        {
            Information.Message = rdv.Message;
            return Page();
        }

        var rds = Global.OpenSession(LDM.Username, LDM.Password);

        Information.Message = rds.Message;

        return rds.Message.Success && Global.Session?.User is not null ? RedirectToPage("/Management/Index") : Page();
    }
}