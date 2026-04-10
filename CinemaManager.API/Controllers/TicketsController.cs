using CinemaManager.API.Data;
using CinemaManager.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class TicketsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public TicketsController(CinemaDbContext db)
    {
        _db = db;
    }

    // GET api/tickets/session/123
    // Public: зрители без JWT
    [HttpGet("session/{sessionId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<OccupiedSeatDto>>> GetOccupiedSeats(int sessionId, CancellationToken ct)
    {
        if (sessionId <= 0) return BadRequest("Некорректный sessionId.");

        var seats = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.SessionId == sessionId)
            .Select(t => new OccupiedSeatDto(t.Row, t.Seat))
            .ToListAsync(ct);

        return Ok(seats);
    }

    // POST api/tickets/buy
    // Public: зрители без JWT
    [HttpPost("buy")]
    [AllowAnonymous]
    public async Task<ActionResult<BuyTicketResultDto>> Buy([FromBody] BuyTicketRequest request, CancellationToken ct)
    {
        if (request.SessionId <= 0) return BadRequest("SessionId обязателен.");
        if (request.Row <= 0 || request.Seat <= 0) return BadRequest("Row и Seat должны быть > 0.");
        if (string.IsNullOrWhiteSpace(request.UserEmail)) return BadRequest("UserEmail обязателен.");

        // Ensure session exists (and keep FK errors user-friendly)
        var sessionExists = await _db.Sessions.AsNoTracking().AnyAsync(s => s.Id == request.SessionId, ct);
        if (!sessionExists) return BadRequest("Сеанс не найден.");

        var email = request.UserEmail.Trim();
        var nowUtc = DateTime.UtcNow;

        var ticket = new Ticket
        {
            SessionId = request.SessionId,
            Row = request.Row,
            Seat = request.Seat,
            UserEmail = email,
            PurchaseDate = nowUtc
        };

        _db.Tickets.Add(ticket);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Unique index prevents double buy; treat as conflict.
            return Conflict("Это место уже занято.");
        }

        return Ok(new BuyTicketResultDto(ticket.Id));
    }

    // GET api/tickets/my?email=a@b.com
    // Public: зрители без JWT
    [HttpGet("my")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MyTicketDto>>> My([FromQuery] string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("email обязателен.");
        var normalized = email.Trim();

        var tickets = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.UserEmail == normalized)
            .OrderByDescending(t => t.PurchaseDate)
            .Include(t => t.Session)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Session)
                .ThenInclude(s => s.Hall)
            .Select(t => new MyTicketDto(
                t.Id,
                t.UserEmail,
                t.PurchaseDate,
                t.SessionId,
                t.Session.StartTime,
                t.Session.TicketPrice,
                t.Row,
                t.Seat,
                t.Session.Movie.Id,
                t.Session.Movie.Title,
                t.Session.Hall.Id,
                t.Session.Hall.Name,
                t.Session.Hall.Type
            ))
            .ToListAsync(ct);

        return Ok(tickets);
    }

    public sealed record OccupiedSeatDto(int Row, int Seat);

    public sealed record BuyTicketRequest(int SessionId, int Row, int Seat, string UserEmail);

    public sealed record BuyTicketResultDto(int TicketId);

    public sealed record MyTicketDto(
        int TicketId,
        string UserEmail,
        DateTime PurchaseDate,
        int SessionId,
        DateTime SessionStartTime,
        decimal TicketPrice,
        int Row,
        int Seat,
        int MovieId,
        string MovieTitle,
        int HallId,
        string HallName,
        string HallType);
}

