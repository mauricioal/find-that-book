using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Infrastructure.OpenLibrary;

public class OpenLibraryClient : IOpenLibraryClient
{
    private readonly HttpClient _httpClient;

    public OpenLibraryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://openlibrary.org/");
    }

    public async Task<IEnumerable<BookCandidate>> SearchBooksAsync(SearchIntent intent, CancellationToken ct = default)
    {
        var queries = new List<string>();
        if (!string.IsNullOrWhiteSpace(intent.Title)) queries.Add($"title={Uri.EscapeDataString(intent.Title)}");
        if (!string.IsNullOrWhiteSpace(intent.Author)) queries.Add($"author={Uri.EscapeDataString(intent.Author)}");
        if (intent.Keywords.Any()) queries.Add($"q={Uri.EscapeDataString(string.Join(" ", intent.Keywords))}");

        if (!queries.Any()) return Enumerable.Empty<BookCandidate>();

        var queryString = string.Join("&", queries);
        var response = await _httpClient.GetFromJsonAsync<OpenLibrarySearchResponse>($"search.json?{queryString}&limit=10", ct);

        if (response?.Docs == null) return Enumerable.Empty<BookCandidate>();

        return response.Docs.Select(doc => new BookCandidate
        {
            Title = doc.Title,
            Authors = doc.AuthorNames ?? new List<string>(),
            FirstPublishYear = doc.FirstPublishYear,
            OpenLibraryId = doc.Key,
            CoverUrl = doc.CoverI != null ? $"https://covers.openlibrary.org/b/id/{doc.CoverI}-M.jpg" : null
        });
    }

    private class OpenLibrarySearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc>? Docs { get; set; }
    }

    private class OpenLibraryDoc
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("author_name")]
        public List<string>? AuthorNames { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverI { get; set; }
    }
}
