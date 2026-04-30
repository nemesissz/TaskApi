using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskApi.Data;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        private string CurrentLogin =>
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notes = await _context.Notes
                .Where(n => n.UserLogin == CurrentLogin)
                .OrderByDescending(n => n.YaranmaTarixi)
                .ToListAsync();
            return Ok(notes.Select(MapDto));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveNoteDto dto)
        {
            var note = new Note
            {
                UserLogin = CurrentLogin,
                Metn = dto.Metn,
                Notlar = dto.Notlar,
                Tamamlanib = dto.Tamamlanib,
                TarixAktiv = dto.TarixAktiv,
                SaatAktiv = dto.SaatAktiv,
                Tarix = dto.Tarix,
                Saat = dto.Saat,
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return Ok(MapDto(note));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveNoteDto dto)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note is null) return NotFound();
            if (note.UserLogin != CurrentLogin) return Forbid();

            note.Metn = dto.Metn;
            note.Notlar = dto.Notlar;
            note.Tamamlanib = dto.Tamamlanib;
            note.TarixAktiv = dto.TarixAktiv;
            note.SaatAktiv = dto.SaatAktiv;
            note.Tarix = dto.Tarix;
            note.Saat = dto.Saat;

            await _context.SaveChangesAsync();
            return Ok(MapDto(note));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note is null) return NotFound();
            if (note.UserLogin != CurrentLogin) return Forbid();

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static NoteDto MapDto(Note n) => new()
        {
            Id = n.Id,
            Metn = n.Metn,
            Notlar = n.Notlar,
            Tamamlanib = n.Tamamlanib,
            YaranmaTarixi = n.YaranmaTarixi.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            TarixAktiv = n.TarixAktiv,
            SaatAktiv = n.SaatAktiv,
            Tarix = n.Tarix,
            Saat = n.Saat,
        };
    }
}
