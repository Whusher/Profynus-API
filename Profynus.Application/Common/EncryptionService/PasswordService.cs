using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using System.Security.Cryptography;
using System.Text;

namespace Profynus.Application.Common.EncryptionService;

public class PasswordService
{
    public string Hash(string plaintext)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            Version = Argon2Version.Nineteen,

            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 4,
            Threads = 4,

            Password = Encoding.UTF8.GetBytes(plaintext),
            Salt = salt,

            HashLength = 32
        };

        using var argon2 = new Argon2(config);

        using SecureArray<byte> hashBytes = argon2.Hash();

        return config.EncodeString(hashBytes.Buffer);
    }

    public bool Verify(string plaintext, string hash)
    {
        return Argon2.Verify(hash, plaintext);
    }
}