using System.Net.Http;
using System.Net.Http.Headers;

namespace CinemaManager.Api;

public static class ApiClient
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(AppSession.BaseUrl.TrimEnd('/') + "/")
    };

    public static HttpClient Http => _client;

    public static void SetBearerToken(string? token)
    {
        AppSession.JwtToken = token;
        _client.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

