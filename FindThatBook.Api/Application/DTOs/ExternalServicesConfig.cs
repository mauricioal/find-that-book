namespace FindThatBook.Api.Application.DTOs;

/// <summary>
/// Configuration for external services and URLs.
/// </summary>
public class ExternalServicesConfig
{
    public OpenLibraryConfig OpenLibrary { get; set; } = new();
    public FrontendConfig Frontend { get; set; } = new();
    public ProjectConfig Project { get; set; } = new();
}

public class OpenLibraryConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string CoversUrl { get; set; } = string.Empty;
}

public class FrontendConfig
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class ProjectConfig
{
    public string RepoUrl { get; set; } = string.Empty;
}
