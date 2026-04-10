using CinemaManager.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class StatsController : ControllerBase
{
    private readonly CinemaDbContext _db;

    public StatsController(CinemaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<StatsDto>> Get([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken ct)
    {
        var movies = await _db.Movies.AsNoTracking().CountAsync(ct);
        var halls = await _db.Halls.AsNoTracking().CountAsync(ct);
        var sessions = await _db.Sessions.AsNoTracking().CountAsync(ct);
        
        var ticketsQuery = _db.Tickets.AsNoTracking();
        if (startDate is not null)
            ticketsQuery = ticketsQuery.Where(t => t.PurchaseDate >= startDate.Value);
        if (endDate is not null)
            ticketsQuery = ticketsQuery.Where(t => t.PurchaseDate <= endDate.Value);

        const decimal pricePerTicket = 500m;

        // 1) Apply filters to Tickets (done above)
        // 2) обязательный Include(Session.Movie)
        // 3) сначала выгружаем в память
        var ticketsList = await ticketsQuery
            .Include(t => t.Session)
            .ThenInclude(s => s.Movie)
            .ToListAsync(ct);

        var tickets = ticketsList.Count;
        var totalIncome = tickets * pricePerTicket;

        // 4) безопасная группировка в памяти
        var movieSales = ticketsList
            .Where(t => t.Session?.Movie is not null)
            .GroupBy(t => t.Session!.Movie!.Title)
            .Select(g => new MovieSalesDto(
                g.Key,
                g.Count(),
                g.Count() * pricePerTicket))
            .OrderByDescending(x => x.TicketsSold)
            .ToList();

        return Ok(new StatsDto(movies, halls, sessions, tickets, totalIncome, movieSales));
    }

    public sealed record StatsDto(
        int Movies,
        int Halls,
        int Sessions,
        int Tickets,
        decimal TotalIncome,
        IReadOnlyList<MovieSalesDto> MovieSales);

    public sealed record MovieSalesDto(string MovieTitle, int TicketsSold, decimal Income);
}

