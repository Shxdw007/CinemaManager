using System.Net.Http;
using System.Net.Http.Json;
using CinemaManager.Web.Client.Models;

namespace CinemaManager.Web.Client.Services;

public sealed class ApiService(HttpClient http)
{
    public async Task<IReadOnlyList<MovieDto>> GetMoviesAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<MovieDto>>("api/movies", ct).ConfigureAwait(false)
           ?? [];

    public async Task<IReadOnlyList<SessionDto>> GetSessionsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<SessionDto>>("api/sessions", ct).ConfigureAwait(false)
           ?? [];

    public Task<MovieDto?> GetMovieAsync(int id, CancellationToken ct = default)
        => http.GetFromJsonAsync<MovieDto>($"api/movies/{id}", ct);

    public Task<SessionDto?> GetSessionAsync(int id, CancellationToken ct = default)
        => http.GetFromJsonAsync<SessionDto>($"api/sessions/{id}", ct);

    public Task<HttpResponseMessage> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct = default)
        => http.PostAsJsonAsync("api/sessions", request, ct);

    public async Task<IReadOnlyList<OccupiedSeatDto>> GetOccupiedSeatsAsync(int sessionId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<OccupiedSeatDto>>($"api/tickets/session/{sessionId}", ct).ConfigureAwait(false)
           ?? [];

    public Task<HttpResponseMessage> BuyTicketAsync(BuyTicketRequest request, CancellationToken ct = default)
        => http.PostAsJsonAsync("api/tickets/buy", request, ct);

    public async Task<IReadOnlyList<MyTicketDto>> GetMyTicketsAsync(string email, CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<MyTicketDto>>($"api/tickets/my?email={Uri.EscapeDataString(email)}", ct).ConfigureAwait(false)
           ?? [];
}

