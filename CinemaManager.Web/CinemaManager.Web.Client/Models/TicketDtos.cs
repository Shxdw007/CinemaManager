namespace CinemaManager.Web.Client.Models;

public sealed record OccupiedSeatDto
{
    public int Row { get; init; }
    public int Seat { get; init; }
}

public sealed record BuyTicketRequest
{
    public int SessionId { get; init; }
    public int Row { get; init; }
    public int Seat { get; init; }
    public string UserEmail { get; init; } = "";
}

public sealed record BuyTicketResultDto
{
    public int TicketId { get; init; }
}

public sealed record MyTicketDto
{
    public int TicketId { get; init; }
    public string UserEmail { get; init; } = "";
    public DateTime PurchaseDate { get; init; }

    public int SessionId { get; init; }
    public DateTime SessionStartTime { get; init; }
    public decimal TicketPrice { get; init; }

    public int Row { get; init; }
    public int Seat { get; init; }

    public int MovieId { get; init; }
    public string MovieTitle { get; init; } = "";

    public int HallId { get; init; }
    public string HallName { get; init; } = "";
    public string HallType { get; init; } = "";
}

