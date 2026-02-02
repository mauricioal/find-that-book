using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Services;

/// <summary>
/// Defines the contract for the book search service.
/// </summary>
public interface IBookSearchService
{
    /// <summary>
    /// Orchestrates the book search process: interprets intent, searches external APIs, matches results, and generates explanations.
    /// </summary>
    /// <param name="query">The raw user query string.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>An enumerable of ranked and explained <see cref="BookCandidate"/> results.</returns>
    Task<IEnumerable<BookCandidate>> SearchAsync(string query, CancellationToken ct = default);
}

/// <summary>
/// Orchestrates the search workflow by coordinating AI, Open Library, and Matching services.
/// </summary>
public class BookSearchService : IBookSearchService
{
    private readonly IAiService _aiService;
    private readonly IOpenLibraryClient _openLibraryClient;
    private readonly IBookMatcher _matcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookSearchService"/> class.
    /// </summary>
    /// <param name="aiService">The AI service for intent extraction and result explanation.</param>
    /// <param name="openLibraryClient">The client for interacting with the Open Library API.</param>
    /// <param name="matcher">The service for ranking and matching candidates.</param>
    public BookSearchService(IAiService aiService, IOpenLibraryClient openLibraryClient, IBookMatcher matcher)
    {
        _aiService = aiService;
        _openLibraryClient = openLibraryClient;
        _matcher = matcher;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BookCandidate>> SearchAsync(string query, CancellationToken ct = default)
    {
        // 1. Extract Intent
        var intent = await _aiService.ExtractSearchIntentAsync(query, ct);
        
        if (!intent.IsValid) return Enumerable.Empty<BookCandidate>();

        // 2. Search OpenLibrary
        var candidates = (await _openLibraryClient.SearchBooksAsync(intent, ct)).ToList();

        // 3. Resolve Primary Authors and Apply Hierarchy in parallel
        var enrichmentTasks = candidates.Select(async candidate =>
        {
            var (primaryAuthors, contributors) = await _openLibraryClient.GetAuthorDetailsAsync(candidate.OpenLibraryId, intent.Title, ct);
            candidate.PrimaryAuthors = primaryAuthors;

            // Derive contributors: Start with those explicitly found (if any), then add those from search result not in primary
            var distinctContributors = new HashSet<string>(contributors, StringComparer.OrdinalIgnoreCase);

            // Search result authors might include contributors not in Work record or explicitly marked
            foreach (var author in candidate.Authors)
            {
                // If this author is NOT in the primary list, assume it's a contributor
                if (!candidate.PrimaryAuthors.Any(pa => pa.Equals(author, StringComparison.OrdinalIgnoreCase)))
                {
                    distinctContributors.Add(author);
                }
            }
            candidate.Contributors = distinctContributors.ToList();

            var matchResult = _matcher.CalculateMatch(query, intent, candidate);
            candidate.Rank = matchResult.Rank;
            candidate.MatchType = matchResult.MatchType;
            candidate.AuthorStatus = matchResult.AuthorStatus;
        });

        await Task.WhenAll(enrichmentTasks);

        // 4. Filter by Highest Rank found
        var validCandidates = candidates
            .Where(c => c.Rank != Domain.Enums.MatchRank.None)
            .ToList();

        if (!validCandidates.Any()) return Enumerable.Empty<BookCandidate>();

        var maxRank = validCandidates.Max(c => c.Rank);
        var rankedCandidates = validCandidates
            .Where(c => c.Rank == maxRank)
            .OrderByDescending(c => c.Rank)
            .Take(5)
            .ToList();

        // 5. AI Generation of grounded explanations
        var results = await _aiService.RankAndExplainResultsAsync(query, intent, rankedCandidates, ct);

        return results;
    }
}
