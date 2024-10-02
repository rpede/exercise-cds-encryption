using System.ComponentModel.DataAnnotations;

public class ReadSecretMessageRequest
{
    [Required]
    public required string Password { get; set; }
}
