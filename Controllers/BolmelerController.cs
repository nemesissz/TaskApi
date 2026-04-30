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
    public class BolmelerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BolmelerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<AppUser?> GetCurrentUser() =>
            await _userManager.FindByIdAsync(CurrentUserId.ToString());

        private async Task<bool> CanManageMuessise(Guid muessiseId)
        {
            var user = await GetCurrentUser();
            if (user is null) return false;
            if (user.Role == "SuperAdmin") return true;
            return user.Role == "Admin" && user.MuessiseId == muessiseId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? muessiseId)
        {
            var user = await GetCurrentUser();
            if (user is null) return Unauthorized();

            IQueryable<Bolme> query = _context.Bolmeler
                .Include(b => b.Muessise)
                .Include(b => b.Users);

            if (user.Role == "SuperAdmin")
            {
                if (muessiseId.HasValue)
                    query = query.Where(b => b.MuessiseId == muessiseId.Value);
            }
            else if (user.Role == "Admin")
            {
                query = query.Where(b => b.MuessiseId == user.MuessiseId);
            }
            else if (user.Role == "BolmeAdmin")
            {
                query = query.Where(b => b.Id == user.BolmeId);
            }
            // Regular users can read all bolmeler for display/lookup purposes

            var bolmeler = await query.ToListAsync();

            return Ok(bolmeler.Select(b => new BolmeDto
            {
                Id = b.Id,
                Ad = b.Ad,
                MuessiseId = b.MuessiseId,
                MuessiseAd = b.Muessise?.Ad,
                AdminUsername = b.AdminUsername,
                UserCount = b.Users.Count
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBolmeDto dto)
        {
            if (!await CanManageMuessise(dto.MuessiseId))
                return Forbid();

            var muessise = await _context.Muessiseler.FindAsync(dto.MuessiseId);
            if (muessise is null) return BadRequest("Müəssisə tapılmadı.");

            if (!string.IsNullOrWhiteSpace(dto.AdminUsername) &&
                await _userManager.FindByNameAsync(dto.AdminUsername) != null)
                return BadRequest("Bu login artıq istifadə olunub.");

            var bolme = new Bolme
            {
                Ad = dto.Ad.Trim(),
                MuessiseId = dto.MuessiseId,
                AdminUsername = dto.AdminUsername?.Trim()
            };

            _context.Bolmeler.Add(bolme);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.AdminUsername) &&
                !string.IsNullOrWhiteSpace(dto.AdminFullName) &&
                !string.IsNullOrWhiteSpace(dto.AdminPassword))
            {
                var adminUser = new AppUser
                {
                    UserName = dto.AdminUsername.Trim(),
                    FullName = dto.AdminFullName.Trim(),
                    Role = "BolmeAdmin",
                    MuessiseId = dto.MuessiseId,
                    BolmeId = bolme.Id
                };

                var result = await _userManager.CreateAsync(adminUser, dto.AdminPassword);
                if (!result.Succeeded)
                {
                    _context.Bolmeler.Remove(bolme);
                    await _context.SaveChangesAsync();
                    return BadRequest(result.Errors.Select(e => e.Description));
                }
            }

            return Ok(new BolmeDto
            {
                Id = bolme.Id,
                Ad = bolme.Ad,
                MuessiseId = bolme.MuessiseId,
                MuessiseAd = muessise.Ad,
                AdminUsername = bolme.AdminUsername,
                UserCount = 0
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bolme = await _context.Bolmeler
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bolme is null) return NotFound();

            if (!await CanManageMuessise(bolme.MuessiseId))
                return Forbid();

            foreach (var user in bolme.Users.ToList())
                await _userManager.DeleteAsync(user);

            _context.Bolmeler.Remove(bolme);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
