namespace CinemaManager.Web.Client.Models;

public sealed record SessionDto
{
    public int Id { get; init; }
    public int MovieId { get; init; }
    public MovieDto Movie { get; init; } = new();
    public int HallId { get; init; }
    public HallDto Hall { get; init; } = new();
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public decimal TicketPrice { get; init; }
}

