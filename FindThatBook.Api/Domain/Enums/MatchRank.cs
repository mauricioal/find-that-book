namespace FindThatBook.Api.Domain.Enums;

public enum MatchRank
{
    None = 0,
    AuthorOnlyFallback = 1,
    NearMatch = 2,
    TitleAndContributorMatch = 3,
    StrongMatch = 4 // Exact/normalized title + primary author
}
