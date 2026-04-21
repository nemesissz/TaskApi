using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly UserManager<AppUser> _userManager;

    public TasksController(ITaskService taskService, UserManager<AppUser> userManager)
    {
        _taskService = taskService;
        _userManager = userManager;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.CreateTaskAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
        => Ok(await _taskService.GetMyTasksAsync(CurrentUserId));

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedToMe()
        => Ok(await _taskService.GetAssignedToMeAsync(CurrentUserId));

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user?.Role != "Admin") return Forbid();
        return Ok(await _taskService.GetAllTasksAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _taskService.GetByIdAsync(id, CurrentUserId));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var result = await _taskService.UpdateTaskAsync(id, dto, CurrentUserId);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TaskItemStatus status)
    {
        await _taskService.UpdateStatusAsync(id, CurrentUserId, status);
        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        await _taskService.CompleteTaskAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
    {
        var comment = await _taskService.AddCommentAsync(id, CurrentUserId, dto);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _taskService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
