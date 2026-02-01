using System.Text.Json;
using FindThatBook.Api.Application.Interfaces;
using FindThatBook.Api.Domain.Entities;
using Microsoft.Extensions.AI;

namespace FindThatBook.Api.Infrastructure.AI;

public class GeminiAiService : IAiService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<GeminiAiService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GeminiAiService(IChatClient chatClient, ILogger<GeminiAiService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
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

    public async Task<IEnumerable<BookCandidate>> ResolveAuthorsAndRankAsync(string rawQuery, IEnumerable<BookCandidate> candidates, CancellationToken ct = default)
    {
        if (!candidates.Any()) return candidates;

        var prompt = $$"""
            You are an expert Librarian AI. A user is searching for a book with this query: "{{rawQuery}}".
            
            Below is a list of candidate books found in our database. Each book includes a list of authors/contributors fetched from the API.
            
            ### Your Tasks:
            1. **Identify Primary Author:** Use your knowledge to determine who the primary author(s) of each work are. Differentiate them from contributors like adaptors, illustrators, or editors.
            2. **Categorize Match:** Assign a MatchRank (5, 4, 3, 2, 1) and MatchType to each book based on the following Hierarchy:
               - **Rank 5 (StrongMatch / ExactTitle):** The user's query contains the EXACT or NORMALIZED title AND the PRIMARY author matches.
               - **Rank 4 (TitleAndContributorMatch / ExactTitle):** The user's query contains the EXACT or NORMALIZED title AND the PRIMARY author doesn't match, but a CONTRIBUTOR (adaptor, illustrator) does.
               - **Rank 3 (NearMatch / NearMatchTitle):** The title matches partially AND the author (primary or contributor) matches.
               - **Rank 2 (AuthorOnlyFallback / AuthorOnly):** Only the author matches (primary or contributor). Return top works by that author.
               - **Rank 1 (TitleMatchOnly / TitleOnly):** The title matches (exact or partial), but the author does NOT match.
            3. **Explain:** Generate a concise 1-2 sentence explanation. State clearly who the primary author is and if the user matched a contributor instead. Cite specific fields (Title, Author). **Explanation MUST NOT be empty.**

            ### Rules:
            - **Strict Normalization:** For Rank 5 and 4, the core title words must be present in the user query. "hobbit" is NOT an exact match for "The Hobbit".
            - **Grounding:** Your explanation MUST be grounded in the data provided.

            ### Output:
            Return ONLY a JSON array of objects with these fields:
            - "Title" (string)
            - "Rank" (int)
            - "MatchType" (string)
            - "AuthorStatus" (string): MUST be one of ["Primary", "Contributor", "Unknown"]
            - "Explanation" (string)
            
            Candidates:
            {{JsonSerializer.Serialize(candidates.Select(c => new { c.Title, c.Authors, c.FirstPublishYear }))}}
            """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: ct);
        var content = CleanLlmJson(response.Text);
        _logger.LogInformation("AI Ranking Response: {Content}", content);

        try 
        {
            var resolvedItems = JsonSerializer.Deserialize<List<ResolvedCandidate>>(content, _jsonOptions);
            
            var candidateList = candidates.ToList();
            var results = new List<BookCandidate>();

            if (resolvedItems != null)
            {
                foreach (var resolved in resolvedItems)
                {
                    var original = candidateList.FirstOrDefault(c => c.Title == resolved.Title);
                    if (original != null)
                    {
                        original.Rank = (Domain.Enums.MatchRank)resolved.Rank;
                        
                        if (Enum.TryParse<Domain.Enums.MatchType>(resolved.MatchType, true, out var matchType))
                            original.MatchType = matchType;
                            
                        if (Enum.TryParse<Domain.Enums.AuthorStatus>(resolved.AuthorStatus, true, out var authorStatus))
                            original.AuthorStatus = authorStatus;
                        else
                            original.AuthorStatus = Domain.Enums.AuthorStatus.Unknown; // Fallback

                        original.Explanation = !string.IsNullOrWhiteSpace(resolved.Explanation) 
                            ? resolved.Explanation 
                            : $"Match found for '{original.Title}' (Rank: {original.Rank}).";
                            
                        results.Add(original);
                    }
                }
            }
            return results.Any() ? results : candidates;
        }
        catch
        {
            return candidates;
        }
    }

    private class ResolvedCandidate
    {
        public string Title { get; set; } = string.Empty;
        public int Rank { get; set; }
        public string MatchType { get; set; } = string.Empty;
        public string AuthorStatus { get; set; } = string.Empty;
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
