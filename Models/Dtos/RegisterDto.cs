using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required, MinLength(2)]
    public string FullName { get; set; } = string.Empty;

    public string? Department { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "İşçi";
}
