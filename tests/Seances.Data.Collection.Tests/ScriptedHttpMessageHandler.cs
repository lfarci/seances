namespace Seances.Data.Collection.Tests;

internal sealed class ScriptedHttpMessageHandler(params (string Path, string Content)[] responses) : HttpMessageHandler
{
    private readonly Queue<(string Path, string Content)> _responses = new(responses);

    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        if (!_responses.TryDequeue(out var response))
            throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
        if (!request.RequestUri!.PathAndQuery.EndsWith(response.Path, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected request ending with '{response.Path}', received '{request.RequestUri.PathAndQuery}'.");
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(response.Content)
        });
    }
}
