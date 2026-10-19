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
    public async Task Production_page_serves_the_Blazor_script_required_for_click_handlers()
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
    }
}
