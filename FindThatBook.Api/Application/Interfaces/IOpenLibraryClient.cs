using FindThatBook.Api.Domain.Entities;

namespace FindThatBook.Api.Application.Interfaces;

/// <summary>
/// Defines the contract for interacting with the Open Library API.
/// </summary>
public interface IOpenLibraryClient
{
    /// <summary>
    /// Searches for books in the Open Library based on the provided search intent.
    /// </summary>
    /// <param name="intent">The structured search intent containing title, author, and keywords.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>An enumerable of preliminary <see cref="BookCandidate"/> results.</returns>
    Task<IEnumerable<BookCandidate>> SearchBooksAsync(SearchIntent intent, CancellationToken ct = default);

    /// <summary>
    /// Retrieves detailed author information for a specific work, categorizing authors into primary and contributors.
    /// </summary>
    /// <param name="workKey">The Open Library key for the work (e.g., "/works/OL123W").</param>
    /// <param name="targetTitle">The title of the book to help disambiguate primary authors from contributors via bio analysis.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing a list of primary authors and a list of contributors.</returns>
    Task<(List<string> PrimaryAuthors, List<string> Contributors)> GetAuthorDetailsAsync(string workKey, string? targetTitle, CancellationToken ct = default);
}
