using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Application.Interfaces;

public record MatchResult(MatchRank Rank, FindThatBook.Api.Domain.Enums.MatchType MatchType, AuthorStatus AuthorStatus);

public interface IBookMatcher
{
    MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate);
}
