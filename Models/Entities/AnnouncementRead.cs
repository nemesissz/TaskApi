public class AnnouncementRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AnnouncementId { get; set; }
    public Announcement Announcement { get; set; } = null!;
    public string UserLogin { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
