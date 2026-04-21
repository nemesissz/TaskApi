using System.ComponentModel.DataAnnotations;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsForAll { get; set; }
    public List<string> Recipients { get; set; } = [];
    public List<string> ReadByLogins { get; set; } = [];
}

public class CreateAnnouncementDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    public bool IsForAll { get; set; } = true;

    public List<string> RecipientLogins { get; set; } = [];
}
