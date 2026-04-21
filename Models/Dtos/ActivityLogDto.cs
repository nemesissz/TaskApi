using System.ComponentModel.DataAnnotations;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserLogin { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddActivityLogDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;
}
