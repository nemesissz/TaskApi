using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

public class Announcement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatorId { get; set; }
    public AppUser Creator { get; set; } = null!;
    public bool IsForAll { get; set; }
    public string RecipientsJson { get; set; } = "[]";
    public ICollection<AnnouncementRead> Reads { get; set; } = [];

    [NotMapped]
    public List<string> Recipients
    {
        get => JsonSerializer.Deserialize<List<string>>(RecipientsJson) ?? [];
        set => RecipientsJson = JsonSerializer.Serialize(value);
    }
}
