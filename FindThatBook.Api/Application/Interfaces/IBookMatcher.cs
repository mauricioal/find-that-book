using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Application.Interfaces;

public record MatchResult(MatchRank Rank, string Explanation);

public interface IBookMatcher
{
    MatchResult CalculateMatch(SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor);
}
