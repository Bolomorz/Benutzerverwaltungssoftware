using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Session;

internal class AuthenticatedUser
{
    protected UserAccount UserAccount;
    protected Role UserRole;

    internal AuthenticatedUser(UserAccount account)
    {
        UserAccount = account;
        UserRole = account.Role is not null ? (Role)account.Role : Role.User;
    }

    internal Role GetRole() => UserRole;
}