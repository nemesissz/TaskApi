using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskApi.Data;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ActivityLogController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            if (user?.Role is not ("Admin" or "BolmeAdmin" or "SuperAdmin")) return Forbid();

            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(logs.Select(l => new ActivityLogDto
            {
                Id = l.Id,
                Type = l.Type,
                UserFullName = l.UserFullName,
                UserLogin = l.UserLogin,
                Description = l.Description,
                CreatedAt = l.CreatedAt
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddActivityLogDto dto)
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            if (user is null) return Unauthorized();

            var log = new ActivityLogItem
            {
                Type = dto.Type,
                UserFullName = user.FullName,
                UserLogin = user.UserName ?? string.Empty,
                Description = dto.Description
            };

            _context.ActivityLogs.Add(log);

            var total = await _context.ActivityLogs.CountAsync();
            if (total > 100)
            {
                var oldest = await _context.ActivityLogs
                    .OrderBy(l => l.CreatedAt)
                    .FirstAsync();
                _context.ActivityLogs.Remove(oldest);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
