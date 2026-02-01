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
        // Arrange
        var rawQuery = "The Hobbit Tolkien";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
        result.AuthorStatus.Should().Be(AuthorStatus.Primary);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnTitleAndContributorMatch_WhenTitleExactButAuthorIsNotPrimary()
    {
        // Arrange
        var rawQuery = "The Hobbit Tolkien";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        // Assert
        result.Rank.Should().Be(MatchRank.TitleAndContributorMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
        result.AuthorStatus.Should().Be(AuthorStatus.Contributor);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnNearMatch_WhenTitlePartialAndAuthorMatches()
    {
        // Arrange
        var rawQuery = "Hobbit Tolkien";
        var intent = new SearchIntent { Title = "Hobbit", Author = "Tolkien" }; 
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.NearMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnAuthorOnlyFallback_WhenTitleDoesNotMatchButAuthorDoes()
    {
        // Arrange
        var rawQuery = "Tolkien";
        var intent = new SearchIntent { Title = "", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Silmarillion", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.AuthorOnlyFallback);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.AuthorOnly);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnStrongMatch_WhenTitleExactAndNoAuthorRequested()
    {
        // Arrange
        var rawQuery = "The Hobbit";
        var intent = new SearchIntent { Title = "The Hobbit", Author = null };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false); 

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
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
        result.Rank.Should().Be(MatchRank.NearMatch); // Should be downgraded
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldMaintainExactMatch_WhenRawQueryContainsCoreTitle()
    {
        // Arrange
        var rawQuery = "read the hobbit please"; // Noise, but exact title is present
        var intent = new SearchIntent { Title = "The Hobbit", Author = null };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch); // Exact title was found in the noise
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
    }

    [Fact]
    public void CalculateMatch_ShouldDowngradeToNearMatch_WhenPartialTitleOnly()
    {
        // Arrange
        var rawQuery = "huckleberry"; // Partial
        var intent = new SearchIntent { Title = "The Adventures of Huckleberry Finn", Author = null }; // AI inferred full title
        var candidate = new BookCandidate 
        { 
            Title = "The Adventures of Huckleberry Finn", 
            Authors = new List<string> { "Mark Twain" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate, isPrimaryAuthor: false);

        // Assert
        // "huckleberry" does NOT contain "adventures huckleberry finn" (core title)
        result.Rank.Should().Be(MatchRank.NearMatch); 
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.NearMatchTitle);
    }
}