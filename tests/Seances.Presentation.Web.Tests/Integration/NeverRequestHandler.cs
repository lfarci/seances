namespace Seances.Presentation.Web.Tests.Integration;

internal sealed class NeverRequestHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Fixture refresh must not perform network requests.");
}
