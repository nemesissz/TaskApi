public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SenderLogin { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverLogin { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool IsEdited { get; set; } = false;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? FileBase64 { get; set; }
}
