using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;

namespace cds_encryption.Controllers;

[ApiController]
[Route("[controller]")]
public class SecretController : ControllerBase
{
    private readonly ILogger<SecretController> _logger;

    public SecretController(ILogger<SecretController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public ActionResult Post([FromBody] SaveSecretMessageRequest request)
    {
        var salt = RandomNumberGenerator.GetBytes(256 / 8);

        var key = KeyDerivation.Pbkdf2(
            request.Password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterationCount: 600_000,
            numBytesRequested: 256 / 8
        );

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // MaxSize = 12
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(request.Message);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // MaxSize = 16

        using (var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize))
        {
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        var encryptedMessage = new EncryptedMessage
        {
            Salt = salt,
            Nonce = nonce,
            CipherText = ciphertext,
            Tag = tag,
        };
        var json = JsonSerializer.Serialize(encryptedMessage);

        // Set a variable to the Documents path.
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Write the string array to a new file named "WriteLines.txt".
        using (
            StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "secret-message.json"))
        )
        {
            outputFile.WriteLine(json);
        }
        return Ok();
    }

    [HttpGet]
    public ActionResult<String> Get([FromQuery] ReadSecretMessageRequest request)
    {
        string json;

        // Set a variable to the Documents path.
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Open the text file using a stream reader.
        using (StreamReader reader = new(Path.Combine(docPath, "secret-message.json")))
        {
            // Read the stream as a string.
            json = reader.ReadToEnd();
        }
        var encryptedMessage = JsonSerializer.Deserialize<EncryptedMessage>(json)!;
        var salt = encryptedMessage.Salt;
        var ciphertext = encryptedMessage.CipherText;
        var nonce = encryptedMessage.Nonce;
        var tag = encryptedMessage.Tag;

        var key = KeyDerivation.Pbkdf2(
            request.Password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterationCount: 600_000,
            numBytesRequested: 256 / 8
        );
        using (var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize))
        {
            var plaintextBytes = new byte[ciphertext.Length];

            aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
