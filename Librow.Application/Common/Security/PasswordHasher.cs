using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Security;
public static class PasswordHasher
{
    private const int SALT_SIZE = 16;
    private const int KEY_SIZE = 32; //advice 64
    private const int ITERATIONS = 100000; //advice 350000
    private static HashAlgorithmName HASH_ALGORITHM = HashAlgorithmName.SHA512;

    public static string Hash(this string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, HASH_ALGORITHM, KEY_SIZE);
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public static bool IsValidWith(this string password, string passwordHash)
    {
        var parts = passwordHash.Split("-");
        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, HASH_ALGORITHM, KEY_SIZE);
        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }

}
