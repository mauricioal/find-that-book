using System.Text.Json;
using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using Microsoft.Extensions.AI;

namespace FindThatBook.Api.Infrastructure.AI;

public class GeminiAiService : IAiService
{
    private readonly IChatClient _chatClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GeminiAiService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<SearchIntent> ExtractSearchIntentAsync(string rawQuery, CancellationToken ct = default)
    {
        var prompt = $"""
            Extract the book search intent from the following messy user query.
            Return ONLY a JSON object with the following fields: "Title", "Author", "Keywords" (a list of strings).
            If a field is unknown, leave it null or empty.
            
            User query: "{rawQuery}"
            """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: ct);
        var content = CleanLlmJson(response.Text);

        try 
        {
            return JsonSerializer.Deserialize<SearchIntent>(content, _jsonOptions) ?? new SearchIntent();
        }
        catch
        {
            return new SearchIntent { Keywords = new List<string> { rawQuery } };
        }
    }

    public async Task<IEnumerable<BookCandidate>> RankAndExplainResultsAsync(string rawQuery, IEnumerable<BookCandidate> candidates, CancellationToken ct = default)
    {
        if (!candidates.Any()) return candidates;

        var candidatesJson = JsonSerializer.Serialize(candidates);
        var prompt = $"""
            A user is searching for a book using this query: "{rawQuery}"
            Below is a list of potential matches from a library database.
            Please re-order these matches by relevance (best match first) and provide a concise one-sentence explanation for EACH match, citing why it fits the query (e.g., "Exact title match", "Matches author and partial title").
            
            Return ONLY the updated list in the same JSON format as the input.
            
            Candidates:
            {candidatesJson}
            """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: ct);
        var content = CleanLlmJson(response.Text);

        try 
        {
            return JsonSerializer.Deserialize<List<BookCandidate>>(content, _jsonOptions) ?? candidates;
        }
        catch
        {
            return candidates;
        }
    }

    private static string CleanLlmJson(string? content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        // Basic cleanup in case LLM wraps JSON in markdown blocks
        if (content.StartsWith("```json")) content = content.Replace("```json", "").Replace("```", "");
        else if (content.StartsWith("```")) content = content.Replace("```", "");
        
        return content.Trim();
    }
}
