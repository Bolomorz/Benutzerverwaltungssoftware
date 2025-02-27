namespace Benutzerverwaltungssoftware.Data;
public enum MID { FailedConnection, Successful, UnauthorizedAccess, NullValue, NotFound, Duplicate, ErrorThrown, FailedValidation }
public class Message
{
    internal bool Success { get; set; }
    internal MID MID { get; set; }
    internal string Error { get; set; }

    internal static Message ConnectionFailed = new(MID.FailedConnection, false, "Datenbankverbindung konnte nicht erfolgreich hergestellt werden.");
    internal static Message OperationSuccessful = new(MID.Successful, true, "Operation erfolgreich ausgefuehrt.");
    internal static Message ConnectionSuccessful = new(MID.Successful, true, "Datenbankverbindung erfolgreich hergestellt.");
    internal static Message ValidationSucccessful = new(MID.Successful, true, "Validation der Eingabe erfolgreich.");
    internal static Message UnauthorizedAccess = new(MID.UnauthorizedAccess, false, "Nicht authorisierter Zugriff.");
    
    internal Message(MID mid, bool success, string error)
    {
        MID = mid;
        Success = success;
        Error = error;
    }

    public override string ToString() => $"{MID}|{Success}|{Error}";
}

public class ReturnDialog
{
    internal Message Message { get; set; }
    internal ReturnDialog(Message message)
    {
        Message = message;
    }
}

public class ReturnDialog<T>
{
    internal Message Message { get; set; }
    internal T? ReturnValue { get; set; }

    internal ReturnDialog(Message message, T? val)
    {
        Message = message;
        ReturnValue = val;
    }
}