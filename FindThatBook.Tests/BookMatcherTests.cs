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
        var result = _matcher.CalculateRank(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Should().Be(MatchRank.StrongMatch);
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
        var result = _matcher.CalculateRank(intent, candidate, isPrimaryAuthor: false);

        // Assert
        result.Should().Be(MatchRank.TitleAndContributorMatch);
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
        var result = _matcher.CalculateRank(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Should().Be(MatchRank.NearMatch);
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
        var result = _matcher.CalculateRank(intent, candidate, isPrimaryAuthor: true);

        // Assert
        result.Should().Be(MatchRank.AuthorOnlyFallback);
    }
}
