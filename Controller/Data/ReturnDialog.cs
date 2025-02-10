namespace Benutzerverwaltungssoftware.Data;
public enum MID { Connection, Successful, UnauthorizedAccess, NullValue, NotFound, Duplicate, ErrorThrown }
public class Message
{
    internal bool Success { get; set; }
    internal MID MID { get; set; }
    internal string Error { get; set; }

    internal static Message FailedToCreateDatabase = new(MID.Connection, false, "failed to create database");
    internal static Message Successful = new(MID.Successful, true, "performing operation successful");
    internal static Message UnauthorizedAccess = new(MID.UnauthorizedAccess, false, "unauthorized access prohibited");
    
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