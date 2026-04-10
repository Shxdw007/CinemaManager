namespace CinemaManager.Web.Client.Models;

public sealed record HallDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int RowsCount { get; init; }
    public int SeatsPerRow { get; init; }
}

