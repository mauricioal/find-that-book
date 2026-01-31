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
        var prompt = $$"""
            You are an expert Librarian AI. Your task is to interpret messy or sparse user queries and translate them into a structured search intent for the OpenLibrary API.
            
            ### Rules
            1. **Authors:** If the query is a famous author's name (e.g., "Tolkien", "King", "Austen"), map it to "Author".
            2. **Titles:** If the query looks like a specific book title (e.g., "1984", "The Hobbit"), map it to "Title".
            3. **Inference:** You are allowed to infer full names if the input is partial but obvious (e.g., "mark huckleberry" -> Author: "Mark Twain", Title: "Adventures of Huckleberry Finn").
            4. **Keywords:** Use this for extra terms like "illustrated", "first edition", or genre.

            ### Examples
            User: "tolkien"
            JSON: { "Title": null, "Author": "J.R.R. Tolkien", "Keywords": [] }

            User: "the hobbit"
            JSON: { "Title": "The Hobbit", "Author": null, "Keywords": [] }

            User: "mark huckleberry"
            JSON: { "Title": "The Adventures of Huckleberry Finn", "Author": "Mark Twain", "Keywords": [] }

            User: "harry potter illustrated"
            JSON: { "Title": "Harry Potter", "Author": "J.K. Rowling", "Keywords": ["illustrated"] }

            ### Task
            User query: "{{rawQuery}}"
            Return ONLY the JSON object.
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
