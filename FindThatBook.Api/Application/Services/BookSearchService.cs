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

        // 3. Resolve Primary Authors and Apply Hierarchy
        foreach (var candidate in candidates)
        {
            var primaryAuthors = await _openLibraryClient.GetPrimaryAuthorsAsync(candidate.OpenLibraryId, ct);
            bool isPrimary = primaryAuthors.Any(pa => intent.Author != null && pa.Contains(intent.Author, StringComparison.OrdinalIgnoreCase));
            
            var matchResult = _matcher.CalculateMatch(query, intent, candidate, isPrimary);
            candidate.Rank = matchResult.Rank;
            candidate.MatchType = matchResult.MatchType;
            candidate.AuthorStatus = matchResult.AuthorStatus;
        }

        // 4. Sort by Rank
        var rankedCandidates = candidates
            .Where(c => c.Rank != Domain.Enums.MatchRank.None)
            .OrderByDescending(c => c.Rank)
            .Take(5)
            .ToList();

        // 5. AI Generation of grounded explanations
        var results = await _aiService.RankAndExplainResultsAsync(query, rankedCandidates, ct);

        return results;
    }
}
