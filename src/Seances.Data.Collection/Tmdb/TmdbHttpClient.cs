using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Seances.Data.Collection.Tmdb;

internal sealed class TmdbHttpClient(HttpClient http, string accessToken, ILogger logger)
{
    private static readonly Uri BaseUrl = new("https://api.themoviedb.org/3/");
    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromSeconds(10);
    private const int MaximumAttempts = 3;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<T> GetAsync<T>(string pathAndQuery, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(BaseUrl, pathAndQuery);
        for (var attempt = 1; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            logger.LogDebug("GET {RequestUrl} (attempt {Attempt}/{MaximumAttempts})", requestUri, attempt, MaximumAttempts);

            using var response = await http.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaximumAttempts)
            {
                var delay = RetryDelay(response);
                logger.LogDebug("TMDB rate limit reached; retrying {RequestUrl} in {RetryDelay}", requestUri, delay);
                await Task.Delay(delay, cancellationToken);
                continue;
            }
            response.EnsureSuccessStatusCode();
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(content, Json, cancellationToken)
                    ?? throw new InvalidDataException($"TMDB returned an empty response for {requestUri}.");
            }
            catch (JsonException error)
            {
                throw new InvalidDataException($"TMDB returned invalid JSON for {requestUri}.", error);
            }
        }
    }

    private static TimeSpan RetryDelay(HttpResponseMessage response)
    {
        var delay = response.Headers.RetryAfter?.Delta ?? DefaultRetryDelay;
        return delay > MaximumRetryDelay ? MaximumRetryDelay : delay;
    }
}
