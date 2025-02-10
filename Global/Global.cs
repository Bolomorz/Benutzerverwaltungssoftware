using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware;

internal static class Global
{
    internal static Session.Session? Session { get; private set; } = null;
    internal static int Year { get; set; } = DateTime.Now.Year;

    internal static ReturnDialog OpenSession(string username, string plainpw)
    {
        Session = new();
        return Session.Authenticate(username, plainpw);
    }
    internal static void CloseSession() => Session = null;
}