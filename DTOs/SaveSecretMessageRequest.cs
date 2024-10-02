using System.ComponentModel.DataAnnotations;

public class SaveSecretMessageRequest
{
    [Required]
    public required string Password { get; set; }

    [Required]
    public required string Message { get; set; }
}
