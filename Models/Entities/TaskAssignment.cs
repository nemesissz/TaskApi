public class TaskAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public Guid AssigneeId { get; set; }
    public AppUser Assignee { get; set; } = null!;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public bool IsNezaretci { get; set; } = false;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}