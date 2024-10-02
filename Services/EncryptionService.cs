using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace cds_encryption.Services;

public interface IEncryptionService
{
    string Decrypt(EncryptedMessage message, string password);
    EncryptedMessage Encrypt(string message, string password);
}

public class EncryptionService : IEncryptionService
{
    public EncryptedMessage Encrypt(string message, string password)
    {
        var salt = GenerateSalt();
        var key = DeriveKeyFromPassword(password, salt);

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(message);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return new EncryptedMessage
        {
            Salt = salt,
            Nonce = nonce,
            CipherText = ciphertext,
            Tag = tag,
        };
    }

    public string Decrypt(EncryptedMessage message, string password)
    {
        var key = DeriveKeyFromPassword(password, message.Salt);
        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var plaintextBytes = new byte[message.CipherText.Length];

        aes.Decrypt(message.Nonce, message.CipherText, message.Tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private byte[] DeriveKeyFromPassword(string password, byte[] salt)
    {
        return KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterationCount: 600_000,
            numBytesRequested: 256 / 8
        );
    }

    private byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(256 / 8);
    }
}
