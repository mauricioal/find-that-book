using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using System.Text.RegularExpressions;

namespace FindThatBook.Api.Application.Services;

/// <summary>
/// Implements logic to match user search intent against book candidates using a hierarchical ranking system.
/// </summary>
public class BookMatcher : IBookMatcher
{
    /// <inheritdoc />
    public MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate)
    {
        var normQueryTitle = Normalize(intent.Title ?? "");
        var normBookTitle = Normalize(candidate.Title);
        var normQueryAuthor = Normalize(intent.Author ?? "");
        
        bool exactTitle = !string.IsNullOrEmpty(normQueryTitle) && normBookTitle == normQueryTitle;
        bool nearMatchTitle = !exactTitle && !string.IsNullOrEmpty(normQueryTitle) && normBookTitle.Contains(normQueryTitle);
        
        bool authorWasRequested = !string.IsNullOrEmpty(normQueryAuthor);
        
        // Check author match against hierarchy
        bool isPrimaryMatch = authorWasRequested && candidate.PrimaryAuthors.Any(a => Normalize(a).Contains(normQueryAuthor));
        bool isContributorMatch = !isPrimaryMatch && authorWasRequested && candidate.Contributors.Any(a => Normalize(a).Contains(normQueryAuthor));
        bool isGeneralMatch = !isPrimaryMatch && !isContributorMatch && authorWasRequested && candidate.Authors.Any(a => Normalize(a).Contains(normQueryAuthor));
        
        var authorStatus = AuthorStatus.Unknown;
        if (isPrimaryMatch) authorStatus = AuthorStatus.Primary;
        else if (isContributorMatch) authorStatus = AuthorStatus.Contributor;
        else if (isGeneralMatch) authorStatus = AuthorStatus.Contributor; // Fallback to lower signal if matched in general list but not primary

        bool authorMatch = authorStatus != AuthorStatus.Unknown;

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

    /// <summary>
    /// Normalizes the input string by converting to lowercase and removing non-alphanumeric characters.
    /// </summary>
    /// <param name="input">The string to normalize.</param>
    /// <returns>The normalized string.</returns>
    private string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input.ToLowerInvariant(), @"[^\w\s]", "").Trim();
    }

}
