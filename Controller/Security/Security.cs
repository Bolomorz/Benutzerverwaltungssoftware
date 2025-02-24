using System.Security.Cryptography;
using System.Text;
using Benutzerverwaltungssoftware.Data;

namespace Benutzerverwaltungssoftware.Security;

internal static class Security
{
    internal static ReturnDialog<UserAccount> GenerateUserData(string username, string password)
    {
        var param = RandomHashParameter.Generate();

        var hash = CalculateHash(param, password, username);

        return new(Message.OperationSuccessful, new()
        {
            Name = username,
            PasswordHash = hash,
            HashParameter = param.ToString()
        });
    }
    internal static ReturnDialog Authenticate(UserAccount account, string password)
    {
        if(account.HashParameter is null || account.Name is null || account.PasswordHash is null) return new(new(MID.NullValue, false, "nullvalue in fields of account"));
        var param = HashParameter.FromString(account.HashParameter);
        if(param is null) return new(new(MID.NullValue, false, "nullvalue trying to convert hashparameter"));
        if(password is null) return new(new(MID.NullValue, false, "password is null"));

        var hash = CalculateHash(param, password, account.Name);

        var auth = Compare(hash, account.PasswordHash);

        return auth ? new(Message.OperationSuccessful) : new(new(MID.UnauthorizedAccess, false, "authorization not successful"));
    }
    private static string CalculateHash(HashParameter param, string plain, string username)
    {
        byte[] salt = Encoding.UTF8.GetBytes(param.ToString() + username);

        var rfc = new Rfc2898DeriveBytes(plain, salt, param.Iterations, HashAlgorithmName.SHA512);
        var key = rfc.GetBytes(param.Length);

        return Convert.ToBase64String(key);
    }
    private static bool Compare(string hash1, string hash2)
    {
        if(hash1 is null || hash2 is null) return false;

        int min = Math.Min(hash1.Length, hash2.Length);
        int result = 0;

        for(int i = 0; i < min; i++) result |= hash1[i] ^ hash2[i];

        return result == 0;
    }
}