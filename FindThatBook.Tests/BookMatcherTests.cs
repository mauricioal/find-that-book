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
    public void CalculateRank_ShouldReturnStrongMatch_WhenTitleExactAndPrimaryAuthorMatches()
    {
        // Arrange
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.Explanation.Should().Contain("Exact title match");
    }

    [Fact]
    public void CalculateRank_ShouldReturnTitleAndContributorMatch_WhenTitleExactButAuthorIsNotPrimary()
    {
        // Arrange
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(intent, candidate, isPrimaryAuthor: false);

        // Assert
        result.Rank.Should().Be(MatchRank.TitleAndContributorMatch);
        result.Explanation.Should().Contain("listed as a contributor");
    }

    [Fact]
    public void CalculateRank_ShouldReturnNearMatch_WhenTitlePartialAndAuthorMatches()
    {
        // Arrange
        var intent = new SearchIntent { Title = "Hobbit", Author = "Tolkien" }; // "Hobbit" is partial of "The Hobbit"
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.NearMatch);
        result.Explanation.Should().Contain("partially matches");
    }

    [Fact]
    public void CalculateRank_ShouldReturnAuthorOnlyFallback_WhenTitleDoesNotMatchButAuthorDoes()
    {
        // Arrange
        var intent = new SearchIntent { Title = "", Author = "Tolkien" };
        var candidate = new BookCandidate 
        { 
            Title = "The Silmarillion", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Rank.Should().Be(MatchRank.AuthorOnlyFallback);
        result.Explanation.Should().Contain("Found via author match");
    }

    [Fact]
    public void CalculateMatch_ShouldReturnStrongMatch_WhenTitleExactAndNoAuthorRequested()
    {
        // Arrange
        var intent = new SearchIntent { Title = "The Hobbit", Author = null };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(intent, candidate, isPrimaryAuthor: false); // isPrimary doesn't matter if author is null

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.Explanation.Should().Contain("Exact title match");
    }
}
