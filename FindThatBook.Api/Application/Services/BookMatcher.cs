using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using System.Text.RegularExpressions;

namespace FindThatBook.Api.Application.Services;

public class BookMatcher : IBookMatcher
{
    public MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor)
    {
        var normQueryTitle = Normalize(intent.Title ?? "");
        var normBookTitle = Normalize(candidate.Title);
        var normQueryAuthor = Normalize(intent.Author ?? "");
        
        bool exactTitle = !string.IsNullOrEmpty(normQueryTitle) && normBookTitle == normQueryTitle;
        bool nearMatchTitle = !exactTitle && !string.IsNullOrEmpty(normQueryTitle) && normBookTitle.Contains(normQueryTitle);
        
        bool authorMatch = !string.IsNullOrEmpty(normQueryAuthor) && candidate.Authors.Any(a => Normalize(a).Contains(normQueryAuthor));
        bool authorWasRequested = !string.IsNullOrEmpty(normQueryAuthor);
        
        var authorStatus = authorMatch ? (isPrimaryAuthor ? AuthorStatus.Primary : AuthorStatus.Contributor) : AuthorStatus.Unknown;

        // VERIFICATION: Check against Raw Query to catch typos or partials
        // If the Intent says "Exact", but the Raw Query doesn't contain the core title, downgrade it.
        if (exactTitle)
        {
            var coreTitle = GetCoreTitle(candidate.Title);
            var normalizedRaw = Normalize(rawQuery);
            if (!normalizedRaw.Contains(coreTitle))
            {
                exactTitle = false;
                nearMatchTitle = true; // Downgrade to Near Match (Typo/Partial)
            }
        }

        // a. Exact title + primary author match (strongest) OR Exact Title Only (if no author requested)
        if (exactTitle && (authorStatus == AuthorStatus.Primary || !authorWasRequested))
        {
            return new MatchResult(MatchRank.StrongMatch, FindThatBook.Api.Domain.Enums.MatchType.ExactTitle, authorStatus);
        }

        // b. Exact title + contributor-only author
        if (exactTitle && authorStatus == AuthorStatus.Contributor)
        {
            return new MatchResult(MatchRank.TitleAndContributorMatch, FindThatBook.Api.Domain.Enums.MatchType.ExactTitle, authorStatus);
        }

        // c. Near-match title + author match OR Near-match title (if no author requested)
        if (nearMatchTitle && (authorMatch || !authorWasRequested))
        {
            return new MatchResult(MatchRank.NearMatch, FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle, authorStatus);
        }

        // d. Author-only fallback
        if (authorMatch && string.IsNullOrEmpty(normQueryTitle))
        {
            return new MatchResult(MatchRank.AuthorOnlyFallback, FindThatBook.Api.Domain.Enums.MatchType.AuthorOnly, authorStatus);
        }

        return new MatchResult(MatchRank.None, FindThatBook.Api.Domain.Enums.MatchType.None, AuthorStatus.Unknown);
    }

    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.ToLowerInvariant(), @"[^\w\s]", "").Trim();
    }

    private string GetCoreTitle(string title)
    {
        // Simple core title extractor: lowercase, remove special chars, remove "the", "a"
        var norm = Normalize(title);
        var words = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w != "the" && w != "a" && w != "an" && w != "of");
        return string.Join(" ", words);
    }
}
