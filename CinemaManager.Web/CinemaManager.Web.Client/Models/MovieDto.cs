namespace CinemaManager.Web.Client.Models;

public sealed record MovieDto
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Genre { get; init; } = "";
    public int Duration { get; init; }
    public string AgeRating { get; init; } = "";
    public string Director { get; init; } = "";
    public bool HasPoster { get; init; }
}

