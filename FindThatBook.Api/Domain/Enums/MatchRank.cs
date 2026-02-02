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
    /// Low relevance; matched on author only without a title match.
    /// </summary>
    AuthorOnlyFallback = 1,

    /// <summary>
    /// Medium relevance; near match on title.
    /// </summary>
    NearMatch = 2,

    /// <summary>
    /// High relevance; exact title match but the author is a contributor.
    /// </summary>
    TitleAndContributorMatch = 3,

    /// <summary>
    /// Highest relevance; exact or normalized title match with a primary author match.
    /// </summary>
    StrongMatch = 4 
}
