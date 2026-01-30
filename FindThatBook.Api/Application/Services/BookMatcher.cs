using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using System.Text.RegularExpressions;

namespace FindThatBook.Api.Application.Services;

public class BookMatcher : IBookMatcher
{
    public MatchRank CalculateRank(SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor)
    {
        var normQueryTitle = Normalize(intent.Title ?? "");
        var normBookTitle = Normalize(candidate.Title);
        var normQueryAuthor = Normalize(intent.Author ?? "");
        
        bool titleMatch = normBookTitle.Contains(normQueryTitle) && !string.IsNullOrEmpty(normQueryTitle);
        bool exactTitle = normBookTitle == normQueryTitle;
        
        bool authorMatch = candidate.Authors.Any(a => Normalize(a).Contains(normQueryAuthor)) && !string.IsNullOrEmpty(normQueryAuthor);

        // a. Exact title + primary author match (strongest)
        if (exactTitle && authorMatch && isPrimaryAuthor)
            return MatchRank.StrongMatch;

        // b. Exact title + contributor-only author
        if (exactTitle && authorMatch && !isPrimaryAuthor)
            return MatchRank.TitleAndContributorMatch;

        // c. Near-match title + author match
        if (titleMatch && authorMatch)
            return MatchRank.NearMatch;

        // d. Author-only fallback
        if (authorMatch && string.IsNullOrEmpty(normQueryTitle))
            return MatchRank.AuthorOnlyFallback;

        return MatchRank.None;
    }

    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.ToLowerInvariant(), @"[^\w\s]", "").Trim();
    }
}
