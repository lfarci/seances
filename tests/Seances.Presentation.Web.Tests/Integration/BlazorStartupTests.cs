using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Seances.Data.Storage;
using Xunit;

namespace Seances.Presentation.Web.Tests.Integration;

public sealed class BlazorStartupTests
{
    [Fact]
    public async Task Production_page_serves_the_interactive_script_and_local_fonts()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Production);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<LocalSnapshotStore>();
                services.AddSingleton(new LocalSnapshotStore(Path.Combine(
                    Path.GetTempPath(), "seances-tests", Guid.NewGuid().ToString("N"), "missing.json")));
            });
        });
        using var client = factory.CreateClient();
        var html = await client.GetStringAsync("/");
        var page = await new HtmlParser().ParseDocumentAsync(html);
        var script = Assert.Single(page.QuerySelectorAll("script[src]"));

        using var response = await client.GetAsync(script.GetAttribute("src"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("javascript", response.Content.Headers.ContentType?.MediaType ?? "");
        Assert.False(string.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));
        Assert.Contains("\"type\":\"server\"", html);

        using var resizerResponse = await client.GetAsync("js/details-resizer.js");
        Assert.Equal(HttpStatusCode.OK, resizerResponse.StatusCode);
        Assert.Contains("javascript", resizerResponse.Content.Headers.ContentType?.MediaType ?? "");

        var fontPreloads = page.QuerySelectorAll("link[rel='preload'][as='font']");
        Assert.Equal(2, fontPreloads.Length);
        foreach (var preload in fontPreloads)
        {
            var path = preload.GetAttribute("href")!;
            Assert.StartsWith("fonts/", path);
            using var fontResponse = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, fontResponse.StatusCode);
            var bytes = await fontResponse.Content.ReadAsByteArrayAsync();
            Assert.Equal("wOF2"u8.ToArray(), bytes.Take(4).ToArray());
        }
    }
}
