public interface ITaskService
{
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, Guid creatorId);
    Task<List<TaskResponseDto>> GetMyTasksAsync(Guid userId);
    Task<List<TaskResponseDto>> GetAssignedToMeAsync(Guid userId);
    Task<List<TaskResponseDto>> GetAllTasksAsync();
    Task<TaskResponseDto> GetByIdAsync(Guid taskId, Guid requesterId);
    Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid requesterId);
    Task UpdateStatusAsync(Guid taskId, Guid userId, TaskItemStatus status);
    Task CompleteTaskAsync(Guid taskId, Guid requesterId, bool value = true);
    Task<CommentDto> AddCommentAsync(Guid taskId, Guid authorId, AddCommentDto dto);
    Task DeleteAsync(Guid taskId, Guid requesterId);
    Task<List<TaskResponseDto>> GetTasksScopedAsync(Guid? bolmeId, Guid? muessiseId);
    Task MarkAsReadAsync(Guid taskId, Guid userId);
    Task<SubTaskDto> AddSubTaskAsync(Guid taskId, Guid requesterId, string title);
    Task<SubTaskDto> ToggleSubTaskAsync(Guid taskId, Guid subTaskId, Guid requesterId);
    Task DeleteSubTaskAsync(Guid taskId, Guid subTaskId, Guid requesterId);
}
