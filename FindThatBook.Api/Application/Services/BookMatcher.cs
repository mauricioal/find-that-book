using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using System.Text.RegularExpressions;

namespace FindThatBook.Api.Application.Services;

public class BookMatcher : IBookMatcher
{
    public MatchResult CalculateMatch(SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor)
    {
        var normQueryTitle = Normalize(intent.Title ?? "");
        var normBookTitle = Normalize(candidate.Title);
        var normQueryAuthor = Normalize(intent.Author ?? "");
        
        bool titleMatch = normBookTitle.Contains(normQueryTitle) && !string.IsNullOrEmpty(normQueryTitle);
        bool exactTitle = normBookTitle == normQueryTitle;
        
        bool authorMatch = !string.IsNullOrEmpty(normQueryAuthor) && candidate.Authors.Any(a => Normalize(a).Contains(normQueryAuthor));
        bool authorWasRequested = !string.IsNullOrEmpty(normQueryAuthor);

        // a. Exact title + primary author match (strongest) OR Exact Title Only (if no author requested)
        if (exactTitle && (authorMatch && isPrimaryAuthor || !authorWasRequested))
        {
            var explanation = authorWasRequested 
                ? $"Exact title match for '{candidate.Title}'; {intent.Author} is verified as the primary author."
                : $"Exact title match for '{candidate.Title}'.";
            
            return new MatchResult(MatchRank.StrongMatch, explanation);
        }

        // b. Exact title + contributor-only author
        if (exactTitle && authorMatch && !isPrimaryAuthor)
        {
            return new MatchResult(MatchRank.TitleAndContributorMatch, 
                $"Exact title match; however, {intent.Author} is listed as a contributor (e.g., illustrator or editor) rather than primary author.");
        }

        // c. Near-match title + author match OR Near-match title (if no author requested)
        if (titleMatch && (authorMatch || !authorWasRequested))
        {
            var explanation = authorWasRequested
                ? $"Title '{candidate.Title}' partially matches your query and the author matches."
                : $"Title '{candidate.Title}' partially matches your query.";

            return new MatchResult(MatchRank.NearMatch, explanation);
        }

        // d. Author-only fallback
        if (authorMatch && string.IsNullOrEmpty(normQueryTitle))
        {
            return new MatchResult(MatchRank.AuthorOnlyFallback, 
                $"Found via author match for '{intent.Author}'; this is one of their popular works.");
        }

        return new MatchResult(MatchRank.None, "Partial match found in library records.");
    }

    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.ToLowerInvariant(), @"[^\w\s]", "").Trim();
    }
}
