using Microsoft.EntityFrameworkCore;
using TaskApi.Data;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context) => _context = context;

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, Guid creatorId)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Deadline = dto.Deadline,
            Note = dto.Note,
            CreatorId = creatorId
        };

        if (dto.AssigneeIds.Any())
        {
            task.Assignments = dto.AssigneeIds.Select(id => new TaskAssignment
            {
                AssigneeId = id
            }).ToList();
        }

        if (dto.Files.Any())
        {
            task.Files = dto.Files.Select(f => new TaskFile
            {
                FileName = f.FileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                Base64Data = f.Base64Data
            }).ToList();
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await MapToResponseDto(task.Id);
    }

    public async Task<List<TaskResponseDto>> GetMyTasksAsync(Guid userId)
    {
        var taskIds = await _context.Tasks
            .Where(t => t.CreatorId == userId)
            .Select(t => t.Id)
            .ToListAsync();

        var result = new List<TaskResponseDto>();
        foreach (var id in taskIds)
            result.Add(await MapToResponseDto(id));

        return result;
    }

    public async Task<List<TaskResponseDto>> GetAssignedToMeAsync(Guid userId)
    {
        var taskIds = await _context.TaskAssignments
            .Where(a => a.AssigneeId == userId)
            .Select(a => a.TaskId)
            .Distinct()
            .ToListAsync();

        var result = new List<TaskResponseDto>();
        foreach (var id in taskIds)
            result.Add(await MapToResponseDto(id));

        return result;
    }

    public async Task<List<TaskResponseDto>> GetAllTasksAsync()
    {
        var taskIds = await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync();

        var result = new List<TaskResponseDto>();
        foreach (var id in taskIds)
            result.Add(await MapToResponseDto(id));

        return result;
    }

    public async Task<TaskResponseDto> GetByIdAsync(Guid taskId, Guid requesterId)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task tapılmadı.");

        bool isCreator = task.CreatorId == requesterId;
        bool isAssignee = task.Assignments.Any(a => a.AssigneeId == requesterId);

        if (!isCreator && !isAssignee)
            throw new UnauthorizedAccessException("Bu tapşırığa giriş icazəniz yoxdur.");

        return await MapToResponseDto(taskId);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid requesterId)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .Include(t => t.Files)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task tapılmadı.");

        if (task.CreatorId != requesterId)
            throw new UnauthorizedAccessException("Yalnız yaradıcı redaktə edə bilər.");

        task.Title = dto.Title;
        task.Note = dto.Note;
        task.Deadline = dto.Deadline;

        // Update assignments
        _context.TaskAssignments.RemoveRange(task.Assignments);
        if (dto.AssigneeIds.Any())
        {
            task.Assignments = dto.AssigneeIds.Select(id => new TaskAssignment
            {
                AssigneeId = id,
                TaskId = taskId
            }).ToList();
        }

        // Replace files
        _context.TaskFiles.RemoveRange(task.Files);
        if (dto.Files.Any())
        {
            task.Files = dto.Files.Select(f => new TaskFile
            {
                FileName = f.FileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                Base64Data = f.Base64Data,
                TaskId = taskId
            }).ToList();
        }

        await _context.SaveChangesAsync();
        return await MapToResponseDto(taskId);
    }

    public async Task UpdateStatusAsync(Guid taskId, Guid userId, TaskItemStatus status)
    {
        var assignment = await _context.TaskAssignments
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssigneeId == userId)
            ?? throw new KeyNotFoundException("Tapşırıq tapılmadı və ya sizə verilməyib.");

        assignment.Status = status;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteTaskAsync(Guid taskId, Guid requesterId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task tapılmadı.");

        if (task.CreatorId != requesterId)
            throw new UnauthorizedAccessException("Yalnız yaradıcı tamamlaya bilər.");

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<CommentDto> AddCommentAsync(Guid taskId, Guid authorId, AddCommentDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task tapılmadı.");

        bool isCreator = task.CreatorId == authorId;
        bool isAssignee = task.Assignments.Any(a => a.AssigneeId == authorId);

        if (!isCreator && !isAssignee)
            throw new UnauthorizedAccessException("Bu tapşırığa giriş icazəniz yoxdur.");

        var author = await _context.Users.FindAsync(authorId)
            ?? throw new KeyNotFoundException("İstifadəçi tapılmadı.");

        var comment = new TaskComment
        {
            TaskId = taskId,
            AuthorId = authorId,
            Text = dto.Text
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            AuthorName = author.FullName,
            AuthorLogin = author.UserName ?? string.Empty,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid taskId, Guid requesterId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task tapılmadı.");

        if (task.CreatorId != requesterId)
            throw new UnauthorizedAccessException("Yalnız yaradıcı silə bilər.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    private async Task<TaskResponseDto> MapToResponseDto(Guid taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.Assignee)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Files)
            .FirstAsync(t => t.Id == taskId);

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority.ToString(),
            Deadline = task.Deadline,
            Note = task.Note,
            CreatedAt = task.CreatedAt,
            IsSelfAssigned = task.IsSelfAssigned,
            IsCompleted = task.IsCompleted,
            CompletedAt = task.CompletedAt,
            CreatorName = task.Creator.FullName,
            CreatorLogin = task.Creator.UserName ?? string.Empty,
            Assignees = task.Assignments.Select(a => new AssigneeDto
            {
                UserId = a.AssigneeId,
                FullName = a.Assignee.FullName,
                Login = a.Assignee.UserName ?? string.Empty,
                Status = a.Status.ToString()
            }).ToList(),
            Comments = task.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    AuthorName = c.Author.FullName,
                    AuthorLogin = c.Author.UserName ?? string.Empty,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList(),
            Files = task.Files.Select(f => new TaskFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                Base64Data = f.Base64Data
            }).ToList()
        };
    }
}
