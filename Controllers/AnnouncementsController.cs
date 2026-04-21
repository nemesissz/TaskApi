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
    public class AnnouncementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AnnouncementsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string CurrentUserLogin =>
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        private async Task<bool> IsAdmin()
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            return user?.Role == "Admin";
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var login = CurrentUserLogin;
            var announcements = await _context.Announcements
                .Include(a => a.Reads)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var mine = announcements.Where(a =>
                a.IsForAll || a.Recipients.Contains(login));

            return Ok(mine.Select(a => MapToDto(a)));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            if (!await IsAdmin()) return Forbid();

            var announcements = await _context.Announcements
                .Include(a => a.Reads)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(announcements.Select(a => MapToDto(a)));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            if (!await IsAdmin()) return Forbid();

            var announcement = new Announcement
            {
                Title = dto.Title,
                Text = dto.Text,
                CreatorId = CurrentUserId,
                IsForAll = dto.IsForAll
            };

            if (!dto.IsForAll)
                announcement.Recipients = dto.RecipientLogins;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(announcement));
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Reads)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement is null) return NotFound();

            var login = CurrentUserLogin;
            if (!announcement.Reads.Any(r => r.UserLogin == login))
            {
                _context.AnnouncementReads.Add(new AnnouncementRead
                {
                    AnnouncementId = id,
                    UserLogin = login
                });
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsAdmin()) return Forbid();

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement is null) return NotFound();

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static AnnouncementDto MapToDto(Announcement a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Text = a.Text,
            CreatedAt = a.CreatedAt,
            IsForAll = a.IsForAll,
            Recipients = a.Recipients,
            ReadByLogins = a.Reads.Select(r => r.UserLogin).ToList()
        };
    }
}
