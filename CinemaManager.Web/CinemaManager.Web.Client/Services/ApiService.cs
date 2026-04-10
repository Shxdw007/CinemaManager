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
}

