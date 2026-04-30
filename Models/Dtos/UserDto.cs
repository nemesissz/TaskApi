using System.ComponentModel.DataAnnotations;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public Guid? MuessiseId { get; set; }
    public Guid? BolmeId { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateUserDto
{
    [Required, MinLength(2)]
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "İşçi";
    public string? NewPassword { get; set; }
    public Guid? MuessiseId { get; set; }
    public Guid? BolmeId { get; set; }
}
