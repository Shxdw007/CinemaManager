namespace CinemaManager.API.Contracts.Sessions;

public sealed class CreateSessionRequest
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal TicketPrice { get; set; }
}

