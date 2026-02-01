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
            Authors = new List<string> { "J.R.R. Tolkien" },
            PrimaryAuthors = new List<string> { "J.R.R. Tolkien" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

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
            Authors = new List<string> { "J.R.R. Tolkien" },
            PrimaryAuthors = new List<string> { "Other Author" },
            Contributors = new List<string> { "J.R.R. Tolkien" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

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
            Authors = new List<string> { "J.R.R. Tolkien" },
            PrimaryAuthors = new List<string> { "J.R.R. Tolkien" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

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
            Authors = new List<string> { "J.R.R. Tolkien" },
            PrimaryAuthors = new List<string> { "J.R.R. Tolkien" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

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
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate); 

        // Assert
        result.Rank.Should().Be(MatchRank.StrongMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
    }
}
