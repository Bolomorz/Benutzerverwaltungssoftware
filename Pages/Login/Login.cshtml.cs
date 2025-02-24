using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Benutzerverwaltungssoftware.Pages.Login;

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
        if(LDM is null || LDM.Username is null || LDM.Username == string.Empty || LDM.Password is null || LDM.Password == string.Empty)
        {
            Information.Message = new(Data.MID.NullValue, false, $"Fehlende Daten. Geben Sie alle Daten ein.");
            return Page();
        }

        var rd = Global.OpenSession(LDM.Username, LDM.Password);

        Information.Message = rd.Message;

        return rd.Message.Success && Global.Session?.User is not null ? RedirectToPage("/Management/Index") : Page();
    }
}