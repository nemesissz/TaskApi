using System.ComponentModel.DataAnnotations;

public class MuessiseDto
{
    public Guid Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public DateTime YaranmaTarixi { get; set; }
    public int UserCount { get; set; }
    public int BolmeCount { get; set; }
}

public class CreateMuessiseDto
{
    [Required]
    public string Ad { get; set; } = string.Empty;
    [Required, MinLength(2)]
    public string AdminFullName { get; set; } = string.Empty;
    [Required]
    public string AdminUsername { get; set; } = string.Empty;
    [Required, MinLength(6)]
    public string AdminPassword { get; set; } = string.Empty;
}

public class BolmeDto
{
    public Guid Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public Guid MuessiseId { get; set; }
    public string? MuessiseAd { get; set; }
    public string? AdminUsername { get; set; }
    public int UserCount { get; set; }
}

public class CreateBolmeDto
{
    [Required]
    public string Ad { get; set; } = string.Empty;
    [Required]
    public Guid MuessiseId { get; set; }
    public string? AdminFullName { get; set; }
    public string? AdminUsername { get; set; }
    public string? AdminPassword { get; set; }
}
