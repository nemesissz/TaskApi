public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? Deadline { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid CreatorId { get; set; }
    public AppUser Creator { get; set; } = null!;

    public bool IsSelfAssigned => Assignments.Count == 0;

    public ICollection<TaskAssignment> Assignments { get; set; } = [];
    public ICollection<TaskComment> Comments { get; set; } = [];
    public ICollection<TaskFile> Files { get; set; } = [];
}