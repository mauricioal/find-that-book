using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Interfaces;

/// <summary>
/// Defines the contract for AI-powered services used in the book search process.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Interprets a raw user query and extracts a structured search intent.
    /// </summary>
    /// <param name="rawQuery">The raw input string from the user.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="SearchIntent"/> object containing the interpreted title, author, and keywords.</returns>
    Task<SearchIntent> ExtractSearchIntentAsync(string rawQuery, CancellationToken ct = default);

    /// <summary>
    /// Ranks a list of book candidates and provides grounded explanations for the matches.
    /// </summary>
    /// <param name="rawQuery">The original raw query from the user.</param>
    /// <param name="intent">The structured search intent used to find the candidates.</param>
    /// <param name="candidates">The list of book candidates to rank and explain.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>An enumerable of <see cref="BookCandidate"/> with populated explanations.</returns>
    Task<IEnumerable<BookCandidate>> RankAndExplainResultsAsync(string rawQuery, SearchIntent intent, IEnumerable<BookCandidate> candidates, CancellationToken ct = default);
}
