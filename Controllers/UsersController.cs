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

        private async Task<bool> IsAdmin()
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            return user?.Role == "Admin";
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.UserName ?? string.Empty,
                Role = u.Role,
                Department = u.Department,
                LastLoginAt = u.LastLoginAt
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName ?? string.Empty,
                Role = user.Role,
                Department = user.Department,
                LastLoginAt = user.LastLoginAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (!await IsAdmin()) return Forbid();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            user.FullName = dto.FullName;
            user.Role = dto.Role;

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

            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName ?? string.Empty,
                Role = user.Role,
                Department = user.Department,
                LastLoginAt = user.LastLoginAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await IsAdmin()) return Forbid();

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
