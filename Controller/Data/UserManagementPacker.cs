using Benutzerverwaltungssoftware.Security;

namespace Benutzerverwaltungssoftware.Data;

internal static class UserManagementPacker
{
    internal static ReturnDialog TestDatabaseConnection()
    {
        try
        {
            using var cmc = new CustomerManagementContext(Global.Year);

            return cmc.UserAccounts is not null && cmc.Customers is not null && cmc.CustomerFiles is not null && cmc.CustomerInvoiceItems is not null && cmc.InvoiceItems is not null ? 
            new(Message.ConnectionSuccessful) : new(Message.ConnectionFailed);
        }
        catch(Exception ex) 
        {
            return new(new(MID.ErrorThrown, false, $"Fehler in TestDatabaseConnection:\n\n{ex}"));
        }
    }
    internal static class Logs
    {
        internal static ReturnDialog WriteLog(string code, string message, ObjectType? type, int? oid)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.Logs is null) return new(Message.ConnectionFailed);

                cmc.Logs.Add(new()
                {
                    Time = DateTime.Now,
                    Code = code,
                    Message = message,
                    Type = type,
                    ObjectID = oid
                });

                return new(Message.OperationSuccessful);
            }
            catch(Exception ex) 
            {
                return new(new(MID.ErrorThrown, false, $"Fehler in WriteLog:\n\n{ex}"));
            }
        }
    }
    internal static class Authentification
    {
        internal static ReturnDialog<UserAccount> Authenticate(string username, string plainpw)
        {
            try
            {
                using var cmc = new CustomerManagementContext(Global.Year);

                if(cmc.UserAccounts is null) return new(Message.ConnectionFailed, null);

                var user = cmc.UserAccounts.FirstOrDefault(x => x.Name == username);

                if(user is null) return new(new(MID.NotFound, false, $"Der Nutzer mit Name {username} wurde nicht gefunden!"), null);

                var auth = Security.Security.Authenticate(user, plainpw);

                return auth.Message.Success ? new(Message.OperationSuccessful, user) : new(auth.Message, null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"Fehler in Authentication.Authenticate:\n\n{ex}"), null);
            }
        }
    }
}