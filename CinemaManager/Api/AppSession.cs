namespace CinemaManager.Api;

public static class AppSession
{
    public static string BaseUrl { get; set; } = "https://localhost:7028";

    public static string? JwtToken { get; set; }

    public static CurrentUser? User { get; set; }
}

public sealed class CurrentUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

