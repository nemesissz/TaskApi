using System.ComponentModel.DataAnnotations;

public class UpdateTaskDto
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public string? Note { get; set; }

    public List<Guid> AssigneeIds { get; set; } = [];

    public DateTime? Deadline { get; set; }

    public List<CreateTaskFileDto> Files { get; set; } = [];

    public int? Priority { get; set; }
}
