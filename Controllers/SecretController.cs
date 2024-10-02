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
        throw new NotImplementedException(
            @"
            TODO:
            1. Derive encryption key from password
            2. Encrypt message using derived key
            3. Save encrypted message to a file
            "
        );
    }

    [HttpGet]
    public ActionResult<String> Get([FromQuery] ReadSecretMessageRequest request)
    {
        throw new NotImplementedException(
            @"
            TODO:
            1. Read encrypted message from file
            2. Derive encryption key from password
            3. Decrypt message using derived key
            4. Return decrypted message
          "
        );
    }
}
