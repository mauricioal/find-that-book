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
            1. **Authors:** If the query is a famous author's name (e.g., "Tolkien", "King", "Austen"), map it to "Author". Don't include author if the query does not indicate an author even if it's a partial match.
            2. **Titles:** If the query looks like a specific book title (e.g., "1984", "The Hobbit"), map it to "Title". Don't include title if the query does not indicate a title even if it's a partial match.
            3. **Inference:** You are allowed to infer full names if the input is partial but obvious (e.g., "mark huckleberry" -> Author: "Mark Twain", Title: "Adventures of Huckleberry Finn").
            4. **Keywords:** Use this for extra terms like "illustrated", "first edition", or genre.
            5. **Explanation:** For EACH attribute (Title, Author, Keywords), you MUST explain *why* you filled it with that value. Explicitly state if you extracted an exact string from the query or if you transformed/inferred it (normalization).

            ### Examples
            User: "tolkien"
            JSON: { 
                "Title": null, 
                "Author": "J.R.R. Tolkien", 
                "Keywords": [],
                "Explanation": {
                    "TitleReason": "No title identified.",
                    "AuthorReason": "Inferred 'J.R.R. Tolkien' from exact match 'tolkien'.",
                    "KeywordsReason": "No keywords found."
                }
            }

            User: "the hobbit"
            JSON: { 
                "Title": "The Hobbit", 
                "Author": null, 
                "Keywords": [],
                "Explanation": {
                    "TitleReason": "Extracted exact title 'the hobbit'.",
                    "AuthorReason": "No author identified.",
                    "KeywordsReason": "No keywords found."
                }
            }

            User: "mark huckleberry"
            JSON: { 
                "Title": "The Adventures of Huckleberry Finn", 
                "Author": "Mark Twain", 
                "Keywords": [],
                "Explanation": {
                    "TitleReason": "Inferred title from 'huckleberry'.",
                    "AuthorReason": "Inferred 'Mark Twain' from 'mark' in context of 'huckleberry'.",
                    "KeywordsReason": "No keywords found."
                }
            }

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

    public async Task<IEnumerable<BookCandidate>> RankAndExplainResultsAsync(string rawQuery, SearchIntent intent, IEnumerable<BookCandidate> candidates, CancellationToken ct = default)
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
            You are an expert Librarian AI. A user searched for a book using a messy or sparse user query.
            
            Original Query (Raw user input): "{{rawQuery}}"
            
            You INTERPRETED the query as:
            Title: {{intent.Title}} (INTERPRETED Explanation: {{intent.Explanation?.TitleReason}})
            Author: {{intent.Author}} (INTERPRETED Explanation: {{intent.Explanation?.AuthorReason}})
            Keywords: {{string.Join(", ", intent.Keywords)}} (INTERPRETED Explanation: {{intent.Explanation?.KeywordsReason}})
            
            Below is a list of results matched by our system (CANDIDATES). Your task is to provide a concise (1-2 sentences) explanation for EACH result, explaining "why this book" matched based on the data provided AND your interpretation of the query.
            For your reasoning compare the Original Query (Raw user input) against INTERPRETED title/author/keywords and CANDIDATE title/author/keywords.

            The explanation should express your reasoning of why the Original Query (Raw user input) matched the candidate based on the INTERPRETED Explanation for each field.

            ### Output Format:
            Return ONLY a JSON array of objects, where each object has all the original fields PLUS the "Explanation" field.
            
            CANDIDATES:
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
