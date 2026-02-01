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
    private readonly IBookMatcher _matcher;

    public BookSearchService(IAiService aiService, IOpenLibraryClient openLibraryClient, IBookMatcher matcher)
    {
        _aiService = aiService;
        _openLibraryClient = openLibraryClient;
        _matcher = matcher;
    }

    public async Task<IEnumerable<BookCandidate>> SearchAsync(string query, CancellationToken ct = default)
    {
        // 1. Extract Intent
        var intent = await _aiService.ExtractSearchIntentAsync(query, ct);
        
        if (!intent.IsValid) return Enumerable.Empty<BookCandidate>();

        // 2. Search OpenLibrary
        var candidates = (await _openLibraryClient.SearchBooksAsync(intent, ct)).ToList();

        // 3. Enrich Candidates with ALL Authors
        foreach (var candidate in candidates)
        {
            candidate.Authors = await _openLibraryClient.GetWorkAuthorsAsync(candidate.OpenLibraryId, ct);
        }

        // 4. Resolve Primary Authors, Apply Hierarchy, and Rank via AI
        // This addresses Requirement 2a (extract info) and 2b (explanations)
        var rankedResults = await _aiService.ResolveAuthorsAndRankAsync(query, candidates, ct);

        // 5. Filter to Top Rank Only (Requirement 4e)
        var bestRankFound = rankedResults
            .Where(c => c.Rank != Domain.Enums.MatchRank.None)
            .MaxBy(c => c.Rank)?.Rank ?? Domain.Enums.MatchRank.None;

        return rankedResults
            .Where(c => c.Rank == bestRankFound && c.Rank != Domain.Enums.MatchRank.None)
            .Take(5)
            .ToList();
    }
}
