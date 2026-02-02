using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Domain.Entities;

/// <summary>
/// Represents a book candidate found during the search process.
/// </summary>
public class BookCandidate
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of all authors associated with the book (including contributors).
    /// </summary>
    public List<string> Authors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of primary authors identified for the work.
    /// </summary>
    public List<string> PrimaryAuthors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of contributors (e.g., illustrators, editors) identified for the work.
    /// </summary>
    public List<string> Contributors { get; set; } = new();

    /// <summary>
    /// Gets or sets the year the book was first published.
    /// </summary>
    public int? FirstPublishYear { get; set; }

    /// <summary>
    /// Gets or sets the unique Open Library identifier (key) for the work.
    /// </summary>
    public string OpenLibraryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the book's cover image.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// Gets or sets the AI-generated explanation for why this book was returned as a match.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the calculated rank of this candidate based on the search intent.
    /// </summary>
    public MatchRank Rank { get; set; } = MatchRank.None;
    
    /// <summary>
    /// Gets or sets the technical match type (e.g., ExactTitle, NearMatch) used for AI grounding.
    /// </summary>
    public FindThatBook.Api.Domain.Enums.MatchType MatchType { get; set; } = Domain.Enums.MatchType.None;

    /// <summary>
    /// Gets or sets the status of the author match (e.g., Primary, Contributor).
    /// </summary>
    public AuthorStatus AuthorStatus { get; set; } = AuthorStatus.Unknown;
}
