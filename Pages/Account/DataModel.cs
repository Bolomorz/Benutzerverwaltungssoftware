using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Pages.Account;

internal static class Information
{
    internal static Message? Message { get; set; }
}

public class LoginDataModel
{
    public required string Username { get; set; }
    public required string Password { get; set; } 

    public ReturnDialog Validate()
    {
        if(!DataModelValidation.ValidateString(Username)) return new(new(MID.FailedValidation, false, "Geben Sie einen Namen ein."));
        if(!DataModelValidation.ValidateString(Password)) return new(new(MID.FailedValidation, false, "Geben Sie ein Passwort ein."));
        return new(Message.ValidationSucccessful);
    }
}