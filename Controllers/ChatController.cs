using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using TaskApi.Data;

namespace TaskApi.Controllers
{
    public static class OnlineTracker
    {
        private static readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();

        public static void Heartbeat(string login) => _lastSeen[login] = DateTime.UtcNow;

        public static List<string> GetOnlineLogins() =>
            _lastSeen
                .Where(kv => (DateTime.UtcNow - kv.Value).TotalMinutes < 2)
                .Select(kv => kv.Key)
                .ToList();
    }


    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string CurrentLogin =>
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string with)
        {
            var login = CurrentLogin;
            var messages = await _context.ChatMessages
                .Where(m => !m.IsDeleted &&
                    ((m.SenderLogin == login && m.ReceiverLogin == with) ||
                     (m.SenderLogin == with && m.ReceiverLogin == login)))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            return Ok(messages.Select(MapDto));
        }

        [HttpPost("messages")]
        public async Task<IActionResult> Send([FromBody] SendChatMessageDto dto)
        {
            var login = CurrentLogin;
            var sender = await _userManager.FindByNameAsync(login);
            var msg = new ChatMessage
            {
                SenderLogin = login,
                SenderName = sender?.FullName ?? login,
                ReceiverLogin = dto.ReceiverLogin,
                ReceiverName = dto.ReceiverName,
                Text = dto.Text,
                FileName = dto.FileName,
                FileType = dto.FileType,
                FileBase64 = dto.FileBase64,
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();
            return Ok(MapDto(msg));
        }

        [HttpPatch("messages/read")]
        public async Task<IActionResult> MarkRead([FromQuery] string from)
        {
            var login = CurrentLogin;
            var unread = await _context.ChatMessages
                .Where(m => m.SenderLogin == from && m.ReceiverLogin == login && !m.IsRead)
                .ToListAsync();
            foreach (var m in unread) m.IsRead = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var login = CurrentLogin;
            var msg = await _context.ChatMessages.FindAsync(id);
            if (msg is null) return NotFound();
            if (msg.SenderLogin != login) return Forbid();
            msg.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("messages/{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditChatMessageDto dto)
        {
            var login = CurrentLogin;
            var msg = await _context.ChatMessages.FindAsync(id);
            if (msg is null) return NotFound();
            if (msg.SenderLogin != login) return Forbid();
            msg.Text = dto.Text;
            msg.IsEdited = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread([FromQuery] string? from)
        {
            var login = CurrentLogin;
            var query = _context.ChatMessages
                .Where(m => m.ReceiverLogin == login && !m.IsRead && !m.IsDeleted);
            if (!string.IsNullOrEmpty(from))
                query = query.Where(m => m.SenderLogin == from);
            var count = await query.CountAsync();
            return Ok(new { count });
        }

        [HttpPost("heartbeat")]
        public IActionResult Heartbeat()
        {
            OnlineTracker.Heartbeat(CurrentLogin);
            return NoContent();
        }

        [HttpGet("online")]
        public IActionResult GetOnline()
        {
            return Ok(OnlineTracker.GetOnlineLogins());
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var login = CurrentLogin;
            var messages = await _context.ChatMessages
                .Where(m => !m.IsDeleted && (m.SenderLogin == login || m.ReceiverLogin == login))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var partners = messages
                .Select(m => m.SenderLogin == login ? m.ReceiverLogin : m.SenderLogin)
                .Distinct()
                .ToList();

            var result = partners.Select(partner =>
            {
                var lastMsg = messages.First(m =>
                    (m.SenderLogin == login && m.ReceiverLogin == partner) ||
                    (m.SenderLogin == partner && m.ReceiverLogin == login));
                var unread = messages.Count(m =>
                    m.SenderLogin == partner && m.ReceiverLogin == login && !m.IsRead);
                return new
                {
                    PartnerLogin = partner,
                    LastMessage = MapDto(lastMsg),
                    UnreadCount = unread,
                };
            });

            return Ok(result);
        }

        private static object MapDto(ChatMessage m) => new
        {
            m.Id,
            m.SenderLogin,
            m.SenderName,
            m.ReceiverLogin,
            m.ReceiverName,
            m.Text,
            m.CreatedAt,
            m.IsRead,
            m.IsDeleted,
            m.IsEdited,
            m.FileName,
            m.FileType,
            m.FileBase64,
        };
    }
}
