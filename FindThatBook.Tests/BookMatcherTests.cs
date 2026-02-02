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
    public void CalculateMatch_ShouldReturnTitleAndContributorMatch_WhenTitleExactButAuthorIsContributor()
    {
        // Arrange
        var rawQuery = "The Hobbit illustrated by Alan Lee";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Alan Lee" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien", "Alan Lee" },
            PrimaryAuthors = new List<string> { "J.R.R. Tolkien" },
            Contributors = new List<string> { "Alan Lee" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

        // Assert
        result.Rank.Should().Be(MatchRank.TitleAndContributorMatch);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.ExactTitle);
        result.AuthorStatus.Should().Be(AuthorStatus.Contributor);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnTitleAndContributorMatch_WhenAuthorIsFoundInGeneralListButNotExplicitlyPrimary()
    {
        // Arrange
        var rawQuery = "Book by Some Guy";
        var intent = new SearchIntent { Title = "Book", Author = "Some Guy" };
        var candidate = new BookCandidate 
        { 
            Title = "Book", 
            Authors = new List<string> { "Some Guy", "Other Author" },
            PrimaryAuthors = new List<string> { "Other Author" } 
            // Some Guy is in Authors but not PrimaryAuthors or Contributors
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate);

        // Assert
        result.Rank.Should().Be(MatchRank.TitleAndContributorMatch);
        result.AuthorStatus.Should().Be(AuthorStatus.Contributor); // It falls back to Contributor if in general list
    }

    [Fact]
    public void CalculateMatch_ShouldReturnNearMatch_WhenTitlePartialAndAuthorMatches()
    {
        // Arrange
        var rawQuery = "Jane Austen Prejudice";
        var intent = new SearchIntent { Title = "Prejudice", Author = "Jane Austen" }; 
        var candidate = new BookCandidate 
        { 
            Title = "Pride and Prejudice", 
            Authors = new List<string> { "Jane Austen" },
            PrimaryAuthors = new List<string> { "Jane Austen" }
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
    public void CalculateMatch_ShouldReturnTitleOnlyFallback_WhenTitleExactAndNoAuthorRequested()
    {
        // Arrange
        var rawQuery = "The Hobbit";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate); 

        // Assert
        result.Rank.Should().Be(MatchRank.TitleOnlyFallback);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.TitleOnly);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnTitleOnlyFallback_WhenTitleNearAndNoAuthorRequested()
    {
        // Arrange
        var rawQuery = "Hobbit";
        var intent = new SearchIntent { Title = "Hobbit", Author = null };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" } 
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate); 

        // Assert
        result.Rank.Should().Be(MatchRank.TitleOnlyFallback);
        result.MatchType.Should().Be(FindThatBook.Api.Domain.Enums.MatchType.TitleOnly);
    }

    [Fact]
    public void CalculateMatch_ShouldReturnNone_WhenAuthorIsRequestedButDoesNotMatch()
    {
        // Arrange
        var rawQuery = "The Hobbit by Brandon Sanderson";
        var intent = new SearchIntent { Title = "The Hobbit", Author = "Brandon Sanderson" };
        var candidate = new BookCandidate 
        { 
            Title = "The Hobbit", 
            Authors = new List<string> { "J.R.R. Tolkien" },
            PrimaryAuthors = new List<string> { "J.R.R. Tolkien" }
        };

        // Act
        var result = _matcher.CalculateMatch(rawQuery, intent, candidate); 

        // Assert
        result.Rank.Should().Be(MatchRank.None);
    }
}