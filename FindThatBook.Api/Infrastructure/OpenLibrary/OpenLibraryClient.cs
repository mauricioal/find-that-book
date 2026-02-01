using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Infrastructure.OpenLibrary;

public class OpenLibraryClient : IOpenLibraryClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenLibraryClient> _logger;

    public OpenLibraryClient(HttpClient httpClient, ILogger<OpenLibraryClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
        _logger.LogInformation("OpenLibrary query: {QueryString}", queryString);

        var response = await _httpClient.GetFromJsonAsync<OpenLibrarySearchResponse>($"search.json?{queryString}&limit=10", ct);
        _logger.LogInformation("OpenLibrary response: {Response}", response);

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

    public async Task<List<string>> GetPrimaryAuthorsAsync(string workKey, CancellationToken ct = default)
    {
        try
        {
            var work = await _httpClient.GetFromJsonAsync<OpenLibraryWork>($"{workKey}.json", ct);
            if (work?.Authors == null) return new List<string>();

            // Fetch authors in parallel to avoid N+1 sequential penalty
            var authorTasks = work.Authors
                .Where(a => a.Author?.Key != null)
                .Select(a => _httpClient.GetFromJsonAsync<OpenLibraryAuthor>($"{a.Author!.Key}.json", ct));

            var authors = await Task.WhenAll(authorTasks);
            
            return authors
                .Where(a => !string.IsNullOrEmpty(a?.Name))
                .Select(a => a!.Name!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching primary authors for work {WorkKey}", workKey);
            return new List<string>();
        }
    }

    private class OpenLibraryWork
    {
        [JsonPropertyName("authors")]
        public List<AuthorRole>? Authors { get; set; }
    }

    private class AuthorRole
    {
        [JsonPropertyName("author")]
        public AuthorKey? Author { get; set; }
    }

    private class AuthorKey
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
    }

    private class OpenLibraryAuthor
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
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
