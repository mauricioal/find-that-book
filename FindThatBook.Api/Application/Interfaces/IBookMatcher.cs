using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Application.Interfaces;

/// <summary>
/// Represents the result of a matching calculation between a search intent and a book candidate.
/// </summary>
/// <param name="Rank">The calculated rank of the match.</param>
/// <param name="MatchType">The specific type of match found (e.g., ExactTitle).</param>
/// <param name="AuthorStatus">The status of the author match (e.g., Primary, Contributor).</param>
public record MatchResult(MatchRank Rank, FindThatBook.Api.Domain.Enums.MatchType MatchType, AuthorStatus AuthorStatus);

/// <summary>
/// Defines the logic for matching search intents against book candidates.
/// </summary>
public interface IBookMatcher
{
    /// <summary>
    /// Calculates the match quality between a user's search intent and a specific book candidate.
    /// </summary>
    /// <param name="rawQuery">The original raw query from the user.</param>
    /// <param name="intent">The structured search intent.</param>
    /// <param name="candidate">The book candidate to evaluate.</param>
    /// <returns>A <see cref="MatchResult"/> containing the rank and metadata of the match.</returns>
    MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate);
}
