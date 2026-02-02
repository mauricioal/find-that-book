namespace FindThatBook.Api.Domain.Enums;

/// <summary>
/// Specifies the type of match found between the search intent and the book candidate.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// No significant match found.
    /// </summary>
    None = 0,

    /// <summary>
    /// Exact match on the book title.
    /// </summary>
    ExactTitle = 1,

    /// <summary>
    /// Partial or near match on the book title.
    /// </summary>
    NearMatchTitle = 2,

    /// <summary>
    /// Match found based on author name only.
    /// </summary>
    AuthorOnly = 3,

    /// <summary>
    /// Match found based on title name only.
    /// </summary>
    TitleOnly = 4
}

/// <summary>
/// Specifies the status of the author match for a candidate.
/// </summary>
public enum AuthorStatus
{
    /// <summary>
    /// Author status is unknown or not applicable.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The matched author is a primary author of the work.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// The matched author is a contributor (e.g., illustrator, editor).
    /// </summary>
    Contributor = 2
}
