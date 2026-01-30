namespace FindThatBook.Api.Domain.Entities;

public class SearchIntent
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public List<string> Keywords { get; set; } = new();
    
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Author) || Keywords.Any();
}
