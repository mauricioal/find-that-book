namespace FindThatBook.Api.Application.DTOs;

/// <summary>
/// Configuration options for Google Gemini AI.
/// </summary>
public class GeminiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public int? MaxOutputTokens { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? TopK { get; set; }
    public long? Seed { get; set; }
}
