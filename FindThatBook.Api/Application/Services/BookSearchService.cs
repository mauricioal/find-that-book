using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Services;

public interface IBookSearchService
{
    Task<IEnumerable<BookCandidate>> SearchAsync(string query, CancellationToken ct = default);
}

public class BookSearchService : IBookSearchService
{
    private readonly IAiService _aiService;
    private readonly IOpenLibraryClient _openLibraryClient;

    public BookSearchService(IAiService aiService, IOpenLibraryClient openLibraryClient)
    {
        _aiService = aiService;
        _openLibraryClient = openLibraryClient;
    }

    public async Task<IEnumerable<BookCandidate>> SearchAsync(string query, CancellationToken ct = default)
    {
        // 1. Extract Intent
        var intent = await _aiService.ExtractSearchIntentAsync(query, ct);
        
        if (!intent.IsValid) return Enumerable.Empty<BookCandidate>();

        // 2. Search OpenLibrary
        var candidates = await _openLibraryClient.SearchBooksAsync(intent, ct);

        // 3. Rank and Explain
        var results = await _aiService.RankAndExplainResultsAsync(query, candidates, ct);

        return results;
    }
}
