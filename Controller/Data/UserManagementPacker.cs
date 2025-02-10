using Benutzerverwaltungssoftware.Security;

namespace Benutzerverwaltungssoftware.Data;

internal static class UserManagementPacker
{
    internal static class Authentification
    {
        internal static ReturnDialog<UserAccount> Authenticate(string username, string plainpw)
        {
            using var umc = new UserManagementContext(Global.Year);

            try
            {
                if(umc.UserAccounts is null) return new(Message.FailedToCreateDatabase, null);

                var user = umc.UserAccounts.FirstOrDefault(x => x.Name == username);

                if(user is null) return new(new(MID.NotFound, false, $"did not find user with username {username}"), null);

                var auth = Security.Security.Authenticate(user, plainpw);

                return auth.Message.Success ? new(Message.Successful, user) : new(auth.Message, null);
            }
            catch(Exception ex)
            {
                return new(new(MID.ErrorThrown, false, $"error in Authentication.Authenticate: {ex}"), null);
            }
        }
    }
}