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
}