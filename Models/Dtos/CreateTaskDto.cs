using System.ComponentModel.DataAnnotations;

public class CreateTaskDto
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public List<Guid> AssigneeIds { get; set; } = [];

    public DateTime? Deadline { get; set; }

    public string? Note { get; set; }

    public List<CreateTaskFileDto> Files { get; set; } = [];
}

public class CreateTaskFileDto
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
}
