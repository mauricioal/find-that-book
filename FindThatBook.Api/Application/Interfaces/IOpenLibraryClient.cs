using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Interfaces;

public interface IOpenLibraryClient
{
    Task<IEnumerable<BookCandidate>> SearchBooksAsync(SearchIntent intent, CancellationToken ct = default);
    Task<List<string>> GetPrimaryAuthorsAsync(string workKey, CancellationToken ct = default);
}
