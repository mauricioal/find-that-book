using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Interfaces;

public interface IOpenLibraryClient
{
    Task<IEnumerable<BookCandidate>> SearchBooksAsync(SearchIntent intent, CancellationToken ct = default);
    Task<(List<string> PrimaryAuthors, List<string> Contributors)> GetAuthorDetailsAsync(string workKey, CancellationToken ct = default);
}
