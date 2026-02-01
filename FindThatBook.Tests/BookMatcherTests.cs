using FindThatBook.Api.Application.Services;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using FluentAssertions;

namespace FindThatBook.Tests;

public class BookMatcherTests
{
    private readonly BookMatcher _matcher;

    public BookMatcherTests()
    {
        _matcher = new BookMatcher();
    }

    [Fact]
    public void CalculateMatch_ShouldReturnStrongMatch_WhenTitleExactAndPrimaryAuthorMatches()
    {
        // Strict: "the hobbit" matches "The Hobbit"
        var rawQuery = "The Hobbit Tolkien";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnNearMatch_WhenArticleMissingInRawQuery()
    {
        // Strict: "hobbit" does NOT match "The Hobbit" exactly
        var rawQuery = "Hobbit Tolkien";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        // Should be NearMatch because "The" is missing
        result.Rank.Should().Be(MatchRank.NearMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnTitleAndContributorMatch_WhenTitleExactButAuthorIsNotPrimary()
    {
        var rawQuery = "The Hobbit Tolkien";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "Christopher Tolkien" } // Contributor/Editor
        };

        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        result.Rank.Should().Be(MatchRank.TitleAndContributorMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
        result.AuthorStatus.Should().Be(AuthorStatus.Contributor);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnNearMatch_WhenTitlePartialAndAuthorMatches()
    {
        var rawQuery = "Huckleberry Twain"; 
        var intent = new SearchIntent { Title = "Huckleberry Finn", Author = "Twain" };
        var candidate = new BookCandidate 
        { 
            Title = "The Adventures of Huckleberry Finn", 
            Authors = new List<string> { "Mark Twain" } 
        };

        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        result.Rank.Should().Be(MatchRank.NearMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnAuthorOnly_WhenTitleMismatch()
    {
        var rawQuery = "Tolkien";
        var intent = new SearchIntent { Title = "", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Silmarillion", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        result.Rank.Should().Be(MatchRank.AuthorOnlyFallback);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.AuthorOnly);
    }

    // --- NEW TESTS FOR TYPO / RAW QUERY LOGIC ---

    [Fact]
    public void CalculateMatch_ShouldDowngradeToNearMatch_WhenTypoInRawQuery()
    {
        // Arrange
        var rawQuery = "hobiit"; // Typo!
        var intent = new SearchIntent { Title = "The Hobbit", Author = null }; // AI corrected it
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        // Assert
        // Since Author is null, and Title is a typo (Partial), it falls to TitleMatchOnly (Rank 1)
        // It cannot be NearMatch (Rank 3) because Requirement 4c says NearMatch requires Author Match.
        result.Rank.Should().Be(MatchRank.TitleMatchOnly);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnTitleMatchOnly_WhenAuthorDoesNotMatch()
    {
        // Arrange
        var rawQuery = "The Hobbit King"; // Wrong Author (Stephen King?)
        var intent = new SearchIntent { Title = "The Hobbit", Author = "King" }; 
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        // Assert
        result.Rank.Should().Be(MatchRank.TitleMatchOnly);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle); // Title matched exactly
        result.AuthorStatus.Should().Be(AuthorStatus.Unknown);
    }
}