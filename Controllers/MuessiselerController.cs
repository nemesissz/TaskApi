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
    public class MuessiselerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MuessiselerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<AppUser?> GetCurrentUser() =>
            await _userManager.FindByIdAsync(CurrentUserId.ToString());

        private async Task<bool> IsSuperAdmin()
        {
            var user = await GetCurrentUser();
            return user?.Role == "SuperAdmin";
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var current = await GetCurrentUser();
            if (current is null) return Unauthorized();

            var muessiseler = await _context.Muessiseler
                .Include(m => m.Users)
                .Include(m => m.Bolmeler)
                .ToListAsync();

            // Non-SuperAdmin users only get id+ad (for display/lookup purposes)
            if (current.Role != "SuperAdmin")
            {
                return Ok(muessiseler.Select(m => new { m.Id, m.Ad }));
            }

            return Ok(muessiseler.Select(m => new MuessiseDto
            {
                Id = m.Id,
                Ad = m.Ad,
                AdminUsername = m.AdminUsername,
                YaranmaTarixi = m.YaranmaTarixi,
                UserCount = m.Users.Count,
                BolmeCount = m.Bolmeler.Count
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var current = await GetCurrentUser();
            if (current is null) return Unauthorized();

            // SuperAdmin hər müəssisəni görə bilər, Admin yalnız özününkünü
            if (current.Role != "SuperAdmin" && current.MuessiseId != id)
                return Forbid();

            var m = await _context.Muessiseler
                .Include(x => x.Users)
                .Include(x => x.Bolmeler)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m is null) return NotFound();

            return Ok(new MuessiseDto
            {
                Id = m.Id,
                Ad = m.Ad,
                AdminUsername = m.AdminUsername,
                YaranmaTarixi = m.YaranmaTarixi,
                UserCount = m.Users.Count,
                BolmeCount = m.Bolmeler.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMuessiseDto dto)
        {
            if (!await IsSuperAdmin()) return Forbid();

            if (await _userManager.FindByNameAsync(dto.AdminUsername) != null)
                return BadRequest("Bu login artıq istifadə olunub.");

            var muessise = new Muessise
            {
                Ad = dto.Ad.Trim(),
                AdminUsername = dto.AdminUsername.Trim(),
                YaranmaTarixi = DateTime.UtcNow
            };

            _context.Muessiseler.Add(muessise);
            await _context.SaveChangesAsync();

            var adminUser = new AppUser
            {
                UserName = dto.AdminUsername.Trim(),
                FullName = dto.AdminFullName.Trim(),
                Role = "Admin",
                MuessiseId = muessise.Id
            };

            var result = await _userManager.CreateAsync(adminUser, dto.AdminPassword);
            if (!result.Succeeded)
            {
                _context.Muessiseler.Remove(muessise);
                await _context.SaveChangesAsync();
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new MuessiseDto
            {
                Id = muessise.Id,
                Ad = muessise.Ad,
                AdminUsername = muessise.AdminUsername,
                YaranmaTarixi = muessise.YaranmaTarixi,
                UserCount = 1,
                BolmeCount = 0
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsSuperAdmin()) return Forbid();

            var muessise = await _context.Muessiseler
                .Include(m => m.Users)
                .Include(m => m.Bolmeler)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (muessise is null) return NotFound();

            foreach (var user in muessise.Users.ToList())
                await _userManager.DeleteAsync(user);

            _context.Muessiseler.Remove(muessise);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
