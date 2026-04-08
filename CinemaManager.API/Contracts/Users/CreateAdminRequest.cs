namespace CinemaManager.API.Contracts.Users;

public sealed class CreateAdminRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

