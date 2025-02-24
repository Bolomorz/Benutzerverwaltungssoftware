using Benutzerverwaltungssoftware.Data;
using Microsoft.AspNetCore.Authentication;

namespace Benutzerverwaltungssoftware.Session;

internal class Session
{
    internal AuthenticatedUser? User { get; private set; }

    internal Session(){}

    internal ReturnDialog Authenticate(string user, string password)
    {
        var rd = UserManagementPacker.Authentification.Authenticate(user, password);

        if(rd.Message.Success && rd.ReturnValue is not null) User = new(rd.ReturnValue);

        return new(rd.Message);
    }

    internal static ReturnDialog TestDatabaseConnection()
    {
        var rd = UserManagementPacker.TestDatabaseConnection();
        
        if(!rd.Message.Success)
        {
            var wl = UserManagementPacker.Logs.WriteLog("TestDatabaseConnection", rd.Message.Error, null, null);
            if(!wl.Message.Success) throw new Exception(wl.Message.Error);
        }

        return rd;
    }
}