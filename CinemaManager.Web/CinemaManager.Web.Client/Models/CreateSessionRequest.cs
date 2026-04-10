namespace CinemaManager.Web.Client.Models;

public sealed record CreateSessionRequest
{
    public int MovieId { get; init; }
    public int HallId { get; init; }
    public DateTime StartTime { get; init; }
    public decimal TicketPrice { get; init; }
}

