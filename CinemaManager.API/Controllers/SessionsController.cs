using CinemaManager.API.Contracts.Sessions;
using CinemaManager.API.Data;
using CinemaManager.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public SessionsController(CinemaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Session>>> Get()
        => Ok(await _db.Sessions
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Session>> Get(int id)
    {
        var session = await _db.Sessions
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);

        return session is null ? NotFound() : Ok(session);
    }

    // Критическое правило ТЗ: запрет пересечений по времени в одном зале.
    // Existing overlap if: (NewStart < ExistingEnd) AND (NewEnd > ExistingStart)
    // NewEnd = NewStart + Movie.Duration + 20 минут уборка
    [HttpPost]
    public async Task<ActionResult<Session>> Post([FromBody] CreateSessionRequest request)
    {
        if (request.MovieId <= 0 || request.HallId <= 0)
            return BadRequest("MovieId и HallId обязательны.");

        var movie = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MovieId);
        if (movie is null) return BadRequest("Фильм не найден.");

        var hallExists = await _db.Halls.AsNoTracking().AnyAsync(h => h.Id == request.HallId);
        if (!hallExists) return BadRequest("Зал не найден.");

        var startUtc = request.StartTime.Kind == DateTimeKind.Utc
            ? request.StartTime
            : DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);

        var endUtc = startUtc.AddMinutes(movie.Duration + 20);

        var hasOverlap = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.HallId == request.HallId)
            .AnyAsync(s => startUtc < s.EndTime && endUtc > s.StartTime);

        if (hasOverlap)
            return Conflict("Нельзя создать сеанс: пересечение по времени в выбранном зале.");

        var session = new Session
        {
            MovieId = request.MovieId,
            HallId = request.HallId,
            StartTime = startUtc,
            EndTime = endUtc,
            TicketPrice = request.TicketPrice
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = session.Id }, session);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Session session)
    {
        if (id != session.Id) return BadRequest("ID в URL не совпадает с ID в теле запроса.");

        _db.Entry(session).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return NotFound();
        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

