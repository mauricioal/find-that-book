namespace FindThatBook.Api.Domain.Entities;

public class SearchIntent
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public List<string> Keywords { get; set; } = new();
    
    public IntentExplainer? Explanation { get; set; }
    
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Author) || Keywords.Any();
}

public class IntentExplainer
{
    public string? TitleReason { get; set; }
    public string? AuthorReason { get; set; }
    public string? KeywordsReason { get; set; }
}
