using FindThatBook.Api.Domain.Enums;

namespace FindThatBook.Api.Domain.Entities;

public class BookCandidate
{
    public string Title { get; set; } = string.Empty;
    public List<string> Authors { get; set; } = new();
    public List<string> PrimaryAuthors { get; set; } = new();
    public List<string> Contributors { get; set; } = new();
    public int? FirstPublishYear { get; set; }
    public string OpenLibraryId { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public MatchRank Rank { get; set; } = MatchRank.None;
    
    // Technical metadata for AI grounding
    public FindThatBook.Api.Domain.Enums.MatchType MatchType { get; set; } = Domain.Enums.MatchType.None;
    public AuthorStatus AuthorStatus { get; set; } = AuthorStatus.Unknown;
}
