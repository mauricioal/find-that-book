using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using System.Text.RegularExpressions;

namespace FindThatBook.Api.Application.Services;

public class BookMatcher : IBookMatcher
{
    public MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor)
    {
        // STRICT Normalization: No article/word removal, just case and punctuation.
        var normQueryTitle = Normalize(intent.Title ?? "");
        var normBookTitle = Normalize(candidate.Title);
        var normQueryAuthor = Normalize(intent.Author ?? "");
        var normRawQuery = Normalize(rawQuery);

        // Title Matching Logic
        bool exactTitle = !string.IsNullOrEmpty(normQueryTitle) && normBookTitle == normQueryTitle;
        bool partialTitle = !exactTitle && !string.IsNullOrEmpty(normQueryTitle) && normBookTitle.Contains(normQueryTitle);

        // Verification: Even if Intent says "Exact", checks if the RAW query actually contains the full title token
        // Strict: "hobbit" is NOT "the hobbit".
        if (exactTitle && !normRawQuery.Contains(normBookTitle))
        {
            exactTitle = false;
            partialTitle = true;
        }

        // Author Matching Logic
        bool authorMatch = !string.IsNullOrEmpty(normQueryAuthor) && candidate.Authors.Any(a => Normalize(a).Contains(normQueryAuthor));
        var authorStatus = authorMatch ? (isPrimaryAuthor ? AuthorStatus.Primary : AuthorStatus.Contributor) : AuthorStatus.Unknown;

        // 4a. Exact/normalized title + primary author match (strongest)
        if (exactTitle && authorStatus == AuthorStatus.Primary)
        {
            return new MatchResult(MatchRank.StrongMatch, FindThatBook.Api.Domain.Enums.MatchType.ExactTitle, authorStatus);
        }

        // 4b. Exact/normalized title + contributor-only author (lower rank)
        if (exactTitle && authorStatus == AuthorStatus.Contributor)
        {
            return new MatchResult(MatchRank.TitleAndContributorMatch, FindThatBook.Api.Domain.Enums.MatchType.ExactTitle, authorStatus);
        }

        // 4c. Near-match title + author match (candidate)
        // Title matches partially AND does the author (either primary or contributor)
        if (partialTitle && authorMatch)
        {
            return new MatchResult(MatchRank.NearMatch, FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle, authorStatus);
        }

        // 4d. Author-only fallback -> return top works by that author
        if (authorMatch && !exactTitle && !partialTitle)
        {
            return new MatchResult(MatchRank.AuthorOnlyFallback, FindThatBook.Api.Domain.Enums.MatchType.AuthorOnly, authorStatus);
        }

        // 4e. Title-only fallback (New lowest rank)
        // Title matches (Exact or Partial), but Author does NOT match.
        if ((exactTitle || partialTitle) && !authorMatch)
        {
            var type = exactTitle ? FindThatBook.Api.Domain.Enums.MatchType.ExactTitle : FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle;
            return new MatchResult(MatchRank.TitleMatchOnly, type, AuthorStatus.Unknown);
        }

        // Default: No valid match category found
        return new MatchResult(MatchRank.None, FindThatBook.Api.Domain.Enums.MatchType.None, AuthorStatus.Unknown);
    }

    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        // Strictly remove punctuation, keep all words including "the", "a", etc.
        return Regex.Replace(input.ToLowerInvariant(), @"[^\w\s]", "").Trim();
    }
}
