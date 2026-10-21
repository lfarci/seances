using System.Net;

namespace Seances.Data.Collection.Tests.Tmdb;

internal sealed class TmdbRoutingHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Queue<(HttpStatusCode Status, string Content)>> _routes = new(StringComparer.Ordinal);

    public List<Uri> Requests { get; } = [];

    public string? LastAuthorization { get; private set; }

    public TmdbRoutingHandler Respond(string path, string content, HttpStatusCode status = HttpStatusCode.OK)
    {
        if (!_routes.TryGetValue(path, out var responses))
        {
            _routes[path] = responses = new Queue<(HttpStatusCode, string)>();
        }
        responses.Enqueue((status, content));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!);
        LastAuthorization = request.Headers.Authorization?.ToString();
        if (!_routes.TryGetValue(request.RequestUri!.AbsolutePath, out var responses) || responses.Count == 0)
        {
            throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
        }
        var (status, content) = responses.Count > 1 ? responses.Dequeue() : responses.Peek();
        var response = new HttpResponseMessage(status) { Content = new StringContent(content) };
        if (status == HttpStatusCode.TooManyRequests)
        {
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.Zero);
        }
        return Task.FromResult(response);
    }
}
