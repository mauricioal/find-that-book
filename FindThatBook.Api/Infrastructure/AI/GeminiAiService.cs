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

        var candidatesData = candidates.Select(c => new 
        {
            c.Title,
            Authors = string.Join(", ", c.Authors),
            c.FirstPublishYear,
            c.MatchType,
            c.AuthorStatus
        });

        var prompt = $$"""
            You are a helpful librarian. A user is searching for a book with this query: "{{rawQuery}}".
            
            Below is a list of results matched by our system. Your task is to provide a concise (1-2 sentences) explanation for EACH result, explaining "why this book" based on the technical match data provided.
            
            ### Grounding Rules:
            - If MatchType is 'ExactTitle', explicitly state that the title matched exactly.
            - If MatchType is 'NearMatchTitle', state that the title is a partial or fuzzy match.
            - If AuthorStatus is 'Primary', mention they are the primary author.
            - If AuthorStatus is 'Contributor', mention they are a contributor (illustrator, editor, etc).
            - Always cite the specific fields that matched.
            
            ### Output Format:
            Return ONLY a JSON array of objects, where each object has all the original fields PLUS the "Explanation" field.
            
            Candidates:
            {{JsonSerializer.Serialize(candidatesData)}}
            """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: ct);
        var content = CleanLlmJson(response.Text);

        try 
        {
            // We only need the explanations back to merge them with our rich candidate objects
            var explainedItems = JsonSerializer.Deserialize<List<ExplainedCandidate>>(content, _jsonOptions);
            
            var candidateList = candidates.ToList();
            if (explainedItems != null)
            {
                for (int i = 0; i < candidateList.Count && i < explainedItems.Count; i++)
                {
                    candidateList[i].Explanation = explainedItems[i].Explanation;
                }
            }
            return candidateList;
        }
        catch
        {
            return candidates;
        }
    }

    private class ExplainedCandidate
    {
        public string Explanation { get; set; } = string.Empty;
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
