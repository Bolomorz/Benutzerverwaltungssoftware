using System.Security.Cryptography;

namespace Benutzerverwaltungssoftware.Security;

static class RandomHashParameter
{
    internal static HashParameter Generate()
    {
        var rand = RandomNumberGenerator.Create();
        int length = RandomNumberGenerator.GetInt32(32, 64);
        int iter = RandomNumberGenerator.GetInt32(10000, 50000);
        byte[] salt = new byte[length];

        rand.GetBytes(salt);

        return new(){ Length = length, Iterations = iter, Salt = Convert.ToBase64String(salt) };
    }
}
class HashParameter
{
    internal required int Length { get; set; }
    internal required int Iterations { get; set; }
    internal required string Salt { get; set; }

    internal static HashParameter? FromString(string value)
    {
        var param = value.Split('.');
        if(param.Length != 3) return null;

        return new(){ Length = Convert.ToInt32(param[0]), Iterations = Convert.ToInt32(param[1]), Salt = param[2] };
    }

    public override string ToString() => $"{Length}.{Iterations}.{Salt}";
}