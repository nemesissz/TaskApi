using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Role { get; set; } = "İşçi";
    public DateTime? LastLoginAt { get; set; }

    public Guid? MuessiseId { get; set; }
    public Guid? BolmeId { get; set; }

    public Muessise? Muessise { get; set; }
    public Bolme? Bolme { get; set; }

    public ICollection<TaskItem> CreatedTasks { get; set; } = [];
    public ICollection<TaskAssignment> Assignments { get; set; } = [];
}
