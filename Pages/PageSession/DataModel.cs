using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages.PageSession;

internal static class Information
{
    internal static Message? Message { get; set; }
}

public class LoginDataModel
{
    public required string Username { get; set; }
    public required string Password { get; set; } 
}