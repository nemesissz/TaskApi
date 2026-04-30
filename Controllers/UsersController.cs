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
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UsersController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<AppUser?> GetCurrentUser() =>
            await _userManager.FindByIdAsync(CurrentUserId.ToString());

        private async Task<bool> IsAdminOrAbove()
        {
            var user = await GetCurrentUser();
            return user?.Role is "Admin" or "BolmeAdmin" or "SuperAdmin";
        }

        private static UserDto MapUser(AppUser u) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Username = u.UserName ?? string.Empty,
            Role = u.Role,
            Department = u.Department,
            MuessiseId = u.MuessiseId,
            BolmeId = u.BolmeId,
            LastLoginAt = u.LastLoginAt
        };

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? muessiseId, [FromQuery] Guid? bolmeId)
        {
            var current = await GetCurrentUser();
            if (current is null) return Unauthorized();

            IQueryable<AppUser> query = _userManager.Users;

            if (muessiseId.HasValue) query = query.Where(u => u.MuessiseId == muessiseId);
            if (bolmeId.HasValue) query = query.Where(u => u.BolmeId == bolmeId);

            var users = await query.ToListAsync();
            return Ok(users.Select(MapUser));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();
            return Ok(MapUser(user));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (!await IsAdminOrAbove()) return Forbid();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            user.FullName = dto.FullName;
            user.Role = dto.Role;
            if (dto.MuessiseId.HasValue) user.MuessiseId = dto.MuessiseId;
            if (dto.BolmeId.HasValue) user.BolmeId = dto.BolmeId;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors.Select(e => e.Description));

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
                if (!pwResult.Succeeded)
                    return BadRequest(pwResult.Errors.Select(e => e.Description));
            }

            return Ok(MapUser(user));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsAdminOrAbove()) return Forbid();

            if (id == CurrentUserId)
                return BadRequest("Özünüzü silə bilməzsiniz.");

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return NoContent();
        }
    }
}
