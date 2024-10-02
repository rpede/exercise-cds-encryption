using cds_encryption.Services;
using Microsoft.AspNetCore.Mvc;

namespace cds_encryption.Controllers;

[ApiController]
[Route("[controller]")]
public class SecretController : ControllerBase
{
    private readonly ILogger<SecretController> _logger;
    private readonly IStorageService _storage;
    private readonly IEncryptionService _encryption;

    public SecretController(
        ILogger<SecretController> logger,
        IStorageService storage,
        IEncryptionService encryption
    )
    {
        _logger = logger;
        _storage = storage;
        _encryption = encryption;
    }

    [HttpPost]
    public ActionResult Post([FromBody] SaveSecretMessageRequest request)
    {
        var encryptedMessage = _encryption.Encrypt(request.Message, request.Password);

        _storage.Save(encryptedMessage);

        return Ok();
    }

    [HttpGet]
    public ActionResult<String> Get([FromQuery] ReadSecretMessageRequest request)
    {
        var encryptedMessage = _storage.Load();

        return _encryption.Decrypt(encryptedMessage, request.Password);
    }
}
