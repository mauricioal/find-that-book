namespace FindThatBook.Api.Domain.Enums;

/// <summary>
/// Defines the ranking levels for search results, ordered by relevance.
/// </summary>
public enum MatchRank
{
    /// <summary>
    /// No rank assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// Lowest relevance; matched on title only without an author match.
    /// </summary>
    TitleOnlyFallback = 1,

    /// <summary>
    /// Low relevance; matched on author only without a title match.
    /// </summary>
    AuthorOnlyFallback = 2,

    /// <summary>
    /// Medium relevance; near match on title.
    /// </summary>
    NearMatch = 3,

    /// <summary>
    /// High relevance; exact title match but the author is a contributor.
    /// </summary>
    TitleAndContributorMatch = 4,

    /// <summary>
    /// Highest relevance; exact or normalized title match with a primary author match.
    /// </summary>
    StrongMatch = 5
}
