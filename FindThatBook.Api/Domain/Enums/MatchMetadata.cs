namespace FindThatBook.Api.Domain.Enums;

public enum MatchType
{
    None = 0,
    ExactTitle = 1,
    NearMatchTitle = 2,
    AuthorOnly = 3
}

public enum AuthorStatus
{
    Unknown = 0,
    Primary = 1,
    Contributor = 2
}
