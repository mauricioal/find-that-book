using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Interfaces;

public interface IAiService
{
    Task<SearchIntent> ExtractSearchIntentAsync(string rawQuery, CancellationToken ct = default);
    Task<IEnumerable<BookCandidate>> RankAndExplainResultsAsync(string rawQuery, IEnumerable<BookCandidate> candidates, CancellationToken ct = default);
}
