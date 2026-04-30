public class SendChatMessageDto
{
    public string ReceiverLogin { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? FileBase64 { get; set; }
}

public class EditChatMessageDto
{
    public string Text { get; set; } = string.Empty;
}
