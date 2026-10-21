using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using AngleSharp.Dom;
using Seances.Data.Collection.Ugc.Models;

namespace Seances.Data.Collection.Ugc.Parsing;

/// <summary>Reads the schema.org Movie JSON-LD block published on UGC film pages.</summary>
public static class UgcFilmJsonLdParser
{
    private const string MovieType = "Movie";

    public static UgcFilmDetailsDto? Parse(IDocument document)
    {
        foreach (var script in document.QuerySelectorAll("script[type='application/ld+json']"))
        {
            var details = TryParse(script.TextContent);
            if (details is not null)
            {
                return details;
            }
        }
        return null;
    }

    private static UgcFilmDetailsDto? TryParse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return FindMovie(document.RootElement) is { } movie ? ToDetails(movie) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonElement? FindMovie(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Select(FindMovie).FirstOrDefault(movie => movie is not null);
        }
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }
        if (String(element, "@type") == MovieType)
        {
            return element;
        }
        return element.TryGetProperty("mainEntity", out var mainEntity) ? FindMovie(mainEntity) : null;
    }

    private static UgcFilmDetailsDto ToDetails(JsonElement movie) => new(
        ParseRuntime(String(movie, "duration")),
        ParseDate(String(movie, "datePublished")),
        Strings(movie, "genre"),
        Names(movie, "director"),
        Names(movie, "actor"),
        Clean(String(movie, "description")),
        ParsePosterUrl(String(movie, "image")));

    private static int? ParseRuntime(string? duration)
    {
        if (duration is null)
        {
            return null;
        }
        try
        {
            var minutes = (int)XmlConvert.ToTimeSpan(duration).TotalMinutes;
            return minutes > 0 ? minutes : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    private static Uri? ParsePosterUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? uri : null;

    private static IReadOnlyList<string> Strings(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return [];
        }
        var values = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().ToArray() : [value];
        return Distinct(values.Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()));
    }

    private static IReadOnlyList<string> Names(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return [];
        }
        var values = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().ToArray() : [value];
        return Distinct(values.Select(item => item.ValueKind switch
        {
            JsonValueKind.String => item.GetString(),
            JsonValueKind.Object => String(item, "name"),
            _ => null
        }));
    }

    private static IReadOnlyList<string> Distinct(IEnumerable<string?> values) => values
        .Select(Clean).OfType<string>().Distinct(StringComparer.Ordinal).ToArray();

    private static string? String(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string? Clean(string? value) =>
        value is null ? null : Regex.Replace(value, @"\s+", " ").Trim() is { Length: > 0 } cleaned ? cleaned : null;
}
