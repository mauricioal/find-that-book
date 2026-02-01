using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Application.Services;
using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;
using Moq;
using FluentAssertions;
using DomainMatchType = FindThatBook.Api.Domain.Enums.MatchType;

namespace FindThatBook.Tests;

public class BookSearchServiceTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IOpenLibraryClient> _openLibraryClientMock;
    private readonly Mock<IBookMatcher> _matcherMock;
    private readonly BookSearchService _service;

    public BookSearchServiceTests()
    {
        _aiServiceMock = new Mock<IAiService>();
        _openLibraryClientMock = new Mock<IOpenLibraryClient>();
        _matcherMock = new Mock<IBookMatcher>();
        _service = new BookSearchService(
            _aiServiceMock.Object, 
            _openLibraryClientMock.Object, 
            _matcherMock.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldOnlyReturnItemsWithHighestRankFound()
    {
        // Arrange
        var query = "test query";
        var intent = new SearchIntent { Title = "Test Title" };
        var candidates = new List<BookCandidate>
        {
            new BookCandidate { Title = "Book 1", OpenLibraryId = "1" },
            new BookCandidate { Title = "Book 2", OpenLibraryId = "2" },
            new BookCandidate { Title = "Book 3", OpenLibraryId = "3" }
        };

        _aiServiceMock.Setup(x => x.ExtractSearchIntentAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        _openLibraryClientMock.Setup(x => x.SearchBooksAsync(intent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _openLibraryClientMock.Setup(x => x.GetAuthorDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<string>(), new List<string>()));

        // Mock different ranks:
        // Book 1: StrongMatch (4)
        // Book 2: NearMatch (2)
        // Book 3: StrongMatch (4)
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[0]))
            .Returns(new MatchResult(MatchRank.StrongMatch, DomainMatchType.ExactTitle, AuthorStatus.Primary));
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[1]))
            .Returns(new MatchResult(MatchRank.NearMatch, DomainMatchType.NearMatchTitle, AuthorStatus.Primary));
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[2]))
            .Returns(new MatchResult(MatchRank.StrongMatch, DomainMatchType.ExactTitle, AuthorStatus.Primary));

        _aiServiceMock.Setup(x => x.RankAndExplainResultsAsync(query, intent, It.IsAny<IEnumerable<BookCandidate>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string q, SearchIntent i, IEnumerable<BookCandidate> cands, CancellationToken ct) => cands);

        // Act
        var results = (await _service.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(c => c.Rank.Should().Be(MatchRank.StrongMatch));
        results.Select(c => c.Title).Should().Contain(new[] { "Book 1", "Book 3" });
        results.Select(c => c.Title).Should().NotContain("Book 2");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnNearMatch_WhenNoStrongMatchExists()
    {
        // Arrange
        var query = "test query";
        var intent = new SearchIntent { Title = "Test Title" };
        var candidates = new List<BookCandidate>
        {
            new BookCandidate { Title = "Book 1", OpenLibraryId = "1" },
            new BookCandidate { Title = "Book 2", OpenLibraryId = "2" }
        };

        _aiServiceMock.Setup(x => x.ExtractSearchIntentAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        _openLibraryClientMock.Setup(x => x.SearchBooksAsync(intent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _openLibraryClientMock.Setup(x => x.GetAuthorDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<string>(), new List<string>()));

        // Mock ranks:
        // Book 1: NearMatch (2)
        // Book 2: AuthorOnlyFallback (1)
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[0]))
            .Returns(new MatchResult(MatchRank.NearMatch, DomainMatchType.NearMatchTitle, AuthorStatus.Primary));
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[1]))
            .Returns(new MatchResult(MatchRank.AuthorOnlyFallback, DomainMatchType.AuthorOnly, AuthorStatus.Primary));

        _aiServiceMock.Setup(x => x.RankAndExplainResultsAsync(query, intent, It.IsAny<IEnumerable<BookCandidate>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string q, SearchIntent i, IEnumerable<BookCandidate> cands, CancellationToken ct) => cands);

        // Act
        var results = (await _service.SearchAsync(query)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results.Should().AllSatisfy(c => c.Rank.Should().Be(MatchRank.NearMatch));
        results.First().Title.Should().Be("Book 1");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoValidCandidatesFound()
    {
        // Arrange
        var query = "test query";
        var intent = new SearchIntent { Title = "Test Title" };
        var candidates = new List<BookCandidate>
        {
            new BookCandidate { Title = "Book 1", OpenLibraryId = "1" }
        };

        _aiServiceMock.Setup(x => x.ExtractSearchIntentAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        _openLibraryClientMock.Setup(x => x.SearchBooksAsync(intent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        _openLibraryClientMock.Setup(x => x.GetAuthorDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<string>(), new List<string>()));

        // Mock rank: None
        _matcherMock.Setup(x => x.CalculateMatch(query, intent, candidates[0]))
            .Returns(new MatchResult(MatchRank.None, DomainMatchType.None, AuthorStatus.Unknown));

        // Act
        var results = (await _service.SearchAsync(query)).ToList();

        // Assert
        results.Should().BeEmpty();
    }
}
