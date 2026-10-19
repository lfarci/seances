using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Seances.Data.Collection.Ugc;

internal sealed class UgcHttpClient(HttpClient http, TimeProvider clock, ILogger logger)
{
    private static readonly TimeSpan RequestInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
    private const int MaximumAttempts = 3;

    private readonly ILogger _logger = logger ?? NullLogger.Instance;
    private DateTimeOffset? _lastRequestAt;

    public async Task<string> GetStringAsync(Uri requestUri, CancellationToken cancellationToken)
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                await WaitForRequestIntervalAsync(cancellationToken);
                _lastRequestAt = clock.GetUtcNow();
                _logger.LogDebug("GET {RequestUrl} (attempt {Attempt}/{MaximumAttempts})",
                    requestUri, attempt, MaximumAttempts);

                using var response = await http.GetAsync(requestUri, cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidDataException($"UGC returned an empty response for {requestUri}.");
                }
                return content;
            }
            catch (HttpRequestException error)
            {
                lastError = error;
            }
            catch (TaskCanceledException error) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = error;
            }

            if (attempt < MaximumAttempts)
            {
                _logger.LogDebug(lastError, "Retrying {RequestUrl} after attempt {Attempt}", requestUri, attempt);
                var delay = TimeSpan.FromTicks(InitialRetryDelay.Ticks * (1L << (attempt - 1)));
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new HttpRequestException($"UGC request failed after {MaximumAttempts} attempts: {requestUri}", lastError);
    }

    private async Task WaitForRequestIntervalAsync(CancellationToken cancellationToken)
    {
        if (_lastRequestAt is not { } lastRequestAt)
        {
            return;
        }
        var wait = RequestInterval - (clock.GetUtcNow() - lastRequestAt);
        if (wait > TimeSpan.Zero)
        {
            await Task.Delay(wait, cancellationToken);
        }
    }
}
