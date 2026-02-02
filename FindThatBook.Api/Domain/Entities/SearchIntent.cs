namespace FindThatBook.Api.Domain.Entities;

/// <summary>
/// Represents the user's interpreted search intent.
/// </summary>
public class SearchIntent
{
    /// <summary>
    /// Gets or sets the interpreted book title from the query.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the interpreted author name from the query.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets list of keywords extracted from the query.
    /// </summary>
    public List<string> Keywords { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the explanation for how the intent was derived.
    /// </summary>
    public IntentExplainer? Explanation { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether the search intent contains valid criteria.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Author) || Keywords.Any();
}

/// <summary>
/// Provides explanations for why specific fields in the search intent were populated.
/// </summary>
public class IntentExplainer
{
    /// <summary>
    /// Gets or sets the reasoning for the interpreted title.
    /// </summary>
    public string? TitleReason { get; set; }

    /// <summary>
    /// Gets or sets the reasoning for the interpreted author.
    /// </summary>
    public string? AuthorReason { get; set; }

    /// <summary>
    /// Gets or sets the reasoning for the extracted keywords.
    /// </summary>
    public string? KeywordsReason { get; set; }
}
