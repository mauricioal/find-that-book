namespace FindThatBook.Api.Domain.Enums;

public enum MatchRank
{
    None = 0,
    TitleMatchOnly = 1, // New fallback: Title matches, Author doesn't
    AuthorOnlyFallback = 2,
    NearMatch = 3,
    TitleAndContributorMatch = 4,
    StrongMatch = 5 // Exact/normalized title + primary author
}
