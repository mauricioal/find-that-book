using FindThatBook.Api.Domain.Entities;
using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Application.Interfaces;

public interface IBookMatcher
{
    MatchRank CalculateRank(SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor);
}
