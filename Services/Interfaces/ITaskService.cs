public interface ITaskService
{
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, Guid creatorId);
    Task<List<TaskResponseDto>> GetMyTasksAsync(Guid userId);
    Task<List<TaskResponseDto>> GetAssignedToMeAsync(Guid userId);
    Task<List<TaskResponseDto>> GetAllTasksAsync();
    Task<TaskResponseDto> GetByIdAsync(Guid taskId, Guid requesterId);
    Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid requesterId);
    Task UpdateStatusAsync(Guid taskId, Guid userId, TaskItemStatus status);
    Task CompleteTaskAsync(Guid taskId, Guid requesterId);
    Task<CommentDto> AddCommentAsync(Guid taskId, Guid authorId, AddCommentDto dto);
    Task DeleteAsync(Guid taskId, Guid requesterId);
    Task<List<TaskResponseDto>> GetTasksScopedAsync(Guid? bolmeId, Guid? muessiseId);
}
