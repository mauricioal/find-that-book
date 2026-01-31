# Find That Book

A smart library discovery service that uses Google Gemini AI to interpret messy user queries and find book matches via the OpenLibrary API. Built with .NET 8 and Clean Architecture principles.

## 🚀 Set Up and Running

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   A Google Gemini API Key (Get it here: [Google AI Studio](https://aistudio.google.com/app/apikey))

### Configuration
1.  Navigate to the `FindThatBook.Api` directory.
2.  Open `appsettings.json`.
3.  Replace the placeholder with your actual API key:
    ```json
    "Gemini": {
      "ApiKey": "YOUR_REAL_API_KEY_HERE"
    }
    ```

### Running the Application
```bash
cd FindThatBook.Api
dotnet run
```
The API will start (typically on `http://localhost:5000` or `https://localhost:5001`).

### Running the Web Application (Frontend)
1.  Navigate to the `FindThatBook.Web` directory.
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Start the development server:
    ```bash
    npm run dev
    ```
4.  Open your browser to `http://localhost:5173`.

### Testing a Request
You can test the search via a browser or curl:
```bash
curl "http://localhost:5000/api/search?query=mark%20huckleberry"
```

---

## 🏗 Implementation Overview

The solution follows **Clean Architecture** to ensure separation of concerns and testability.

*   **Domain**: Contains core entities (`BookCandidate`, `SearchIntent`) and Enums (`MatchRank`). No external dependencies.
*   **Application**: Defines Use Cases (`BookSearchService`) and Interfaces (`IAiService`, `IOpenLibraryClient`). It orchestrates the flow.
*   **Infrastructure**: Implements the interfaces.
    *   **AI**: Uses `GeminiDotnet` and `Microsoft.Extensions.AI` to communicate with Google Gemini.
    *   **OpenLibrary**: Uses `HttpClient` to fetch data from OpenLibrary.
*   **Web**: The API entry point (Controllers).

### The Search Workflow
1.  **Extract**: The raw user query (e.g., "tolkien hobbit illustrated") is sent to Gemini to extract structured intent (Title: "The Hobbit", Author: "Tolkien", Keywords: ["illustrated"]).
2.  **Search**: The structured intent is used to query the OpenLibrary Search API.
3.  **Refine**: Additional details (Work & Author metadata) are fetched to identify primary authors vs. contributors.
4.  **Rank**: A deterministic `BookMatcher` applies the specific matching hierarchy (Exact Title > Primary Author > etc.).
5.  **Explain**: The top candidates are sent back to Gemini to generate a human-readable explanation ("Exact title match; Tolkien is primary author").

---

## 💡 Design Decisions & Assumptions

*   **Hybrid Ranking Approach**: While LLMs are powerful, strict logic rules (like "Exact Title + Primary Author must be rank 1") are safer implemented in code. I used C# logic for the "Matching Hierarchy" to ensure strict adherence to the requirements, while using AI for the fuzzy "understanding" and "explaining" parts.
*   **Two-Step AI Pipeline**: Instead of a single "Tool Call", I split the process. First, extract intent to get clean search terms. Second, summarize results. This provides better control over the OpenLibrary API usage.
*   **Data Quality Handling**: OpenLibrary often mixes authors and illustrators. The system explicitly fetches `/works/{id}` data to differentiate `author` (primary) from other roles, ensuring we don't rank a book high just because an illustrator matches the author query.
*   **Monolithic Structure**: For the scale of this challenge, I used a single project solution with logical layering (Folders) rather than multiple `.csproj` files to keep the development loop fast while maintaining architectural cleanliness.

---

## ✨ Features Implemented

*   **Messy Query Parsing**: Handles sparse queries ("dickens"), dense queries ("tolkien hobbit 1937"), and ambiguous input using LLM extraction.
*   **Structured Search**: Maps natural language to specific OpenLibrary API parameters (`title`, `author`, `q`).
*   **Smart Ranking**: Implements the required matching hierarchy:
    *   Strong Match (Exact Title + Primary Author)
    *   Contributor Match
    *   Near Match (Partial Title)
    *   Author Fallback
*   **AI Explanations**: Returns a "Why it matched" sentence for every result, grounded in the actual data.
*   **Canonical Data**: Resolves works to their canonical versions to avoid duplicates.

---

## 🧪 Testing Strategy

*   **Unit Tests (`FindThatBook.Tests`)**:
    *   Focused on the **Core Domain Logic**, specifically the `BookMatcher`.
    *   Verified that the "Matching Hierarchy" works correctly (e.g., ensuring an exact title match with a primary author outranks a contributor match).
    *   Used `xUnit` and `FluentAssertions` for readable tests.
*   **Manual Integration Testing**:
    *   Verified the full pipeline end-to-end using real queries against the OpenLibrary API and Gemini to ensure the integration points (HTTP Client, JSON parsing) function correctly.

---

## 🔮 Future Improvements



1.  **Multi-LLM Support**: Leverage the `Microsoft.Extensions.AI` abstraction to easily integrate other providers such as **OpenAI** (GPT-4o) or **Anthropic** (Claude 3.5 Sonnet) as alternatives to Gemini.

2.  **Resilience**: Add `Polly` for retrying failed OpenLibrary or AI requests (Rate limiting is common).

3.  **Caching**: Implement `IMemoryCache` or Redis for search results to reduce API costs and latency.

4.  **Web Frontend**: Build the React UI `FindThatBook.Web` to consume this API.

5.  **Integration Tests**: Add `TestServer` tests that mock external HTTP calls to verify the full `BookSearchService` orchestration.
