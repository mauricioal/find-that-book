namespace FindThatBook.Api.Domain.Entities;

public class BookCandidate
{
    public string Title { get; set; } = string.Empty;
    public List<string> Authors { get; set; } = new();
    public int? FirstPublishYear { get; set; }
    public string OpenLibraryId { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Explanation { get; set; } = string.Empty;
}
