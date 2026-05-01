public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSelfAssigned { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorLogin { get; set; } = string.Empty;
    public List<AssigneeDto> Assignees { get; set; } = [];
    public List<CommentDto> Comments { get; set; } = [];
    public List<TaskFileDto> Files { get; set; } = [];
}

public class AssigneeDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsNezaretci { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TaskFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
}
