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

---

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
*   **Data Quality Handling**: OpenLibrary often mixes authors and illustrators or other types of collaborators. The system fetches `/works/{id}` and `/author/{id}` data and uses **bio analysis** (checking if the title appears in the author's bio) and role checks to probabilistically differentiate `primary` authors from contributors.
*   **Project Structure**: The solution is divided into three projects:
    *   `FindThatBook.Api`: The backend API (.NET 8).
    *   `FindThatBook.Web`: The frontend UI (React + Vite).
    *   `FindThatBook.Tests`: Unit tests for the backend logic.

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
    *   Focused on the **Core Domain Logic**, specifically the `BookMatcher` and `BookSearchService`.
    *   Verified that the "Matching Hierarchy" works correctly (e.g., ensuring an exact title match with a primary author outranks a contributor match).
    *   Mocked external dependencies (`IOpenLibraryClient`, `IAiService`) to test orchestration logic in isolation.
    *   Used `xUnit`, `FluentAssertions`, and `Moq` for readable and robust tests.
*   **Manual Integration Testing**:
    *   Verified the full pipeline end-to-end using real queries against the OpenLibrary API and Gemini to ensure the integration points (HTTP Client, JSON parsing) function correctly.

---

## 🧠 Context Engineering Practices

This project utilizes advanced **Context Engineering** to ensure consistency and quality during AI-assisted development:

*   **`gemini.md`**: A root-level configuration file that defines the project's architectural rules, coding standards (naming conventions, DI usage), and testing requirements. This serves as the "source of truth" for AI agents working on the codebase.
*   **`.gemini/`**: A folder containing reusable, high-quality prompts for specialized tasks, such as:
    *   `code-review-backend.md`: For standardized architectural and performance reviews of .NET code.
    *   `code-review-frontend.md`: For reviewing React/TypeScript frontend code against project standards.
    *   `security-audit.md`: For performing OWASP-aligned security scans of the solution.
    *   `generate-docstrings-csharp.md`: For generating .NET 8 compliant XML documentation.
*   **`/docs/`**: Comprehensive documentation covering the technical stack, architectural patterns, and feature-specific implementation plans (e.g., `005FixPrimaryAuthorLogicPLAN.md`), ensuring that both human developers and AI agents have clear guidance on the project's evolution.

---

## 🔮 Future Improvements

1.  **Configuration Management**: Extract all external service URLs (OpenLibrary, etc.) into `appsettings.json` or Environment Variables for better flexibility across environments.
2.  **Architectural Scaling**: Split the `FindThatBook.Api` project into separate Class Library projects (`FindThatBook.Domain`, `FindThatBook.Application`, `FindThatBook.Infrastructure`) to strictly enforce Clean Architecture boundaries.
3.  **Frontend Polish**: Refactor the React project structure to follow feature-based folders and improve component reusability/design.
4.  **Cloud Deployment**: Deploy the solution to **Azure** using cost-effective services like **Azure Container Apps** (consumption plan) or **App Service** (Linux Free Tier) and **Static Web Apps** for the frontend.
5.  **Observability**: Integrate **OpenTelemetry** with tools like **Aspire Dashboard**, **Application Insights**, or **Grafana/Prometheus** for distributed tracing and metrics across frontend and backend.
6.  **Security**: Secure the API with **OAuth2/OIDC** (e.g., Auth0 or Azure AD B2C) and ensure strict CORS/CSP policies between the frontend and backend.
7.  **E2E Testing**: Implement **Playwright** tests for the frontend to verify critical user flows (Search -> Results -> Details).
8.  **Refined Explanations**: Fine-tune the AI prompts or use few-shot prompting to ensure the "Why it matched" explanations strictly adhere to the requested format and tone.
9.  **AI Reranking**: Introduce a final re-ranking step where the LLM evaluates the top 5 candidates deeply to adjust the order based on subtle query nuances before returning the final top 5.
10. **Multi-LLM Support**: Leverage `Microsoft.Extensions.AI` to easily swap providers (OpenAI, Anthropic) or implement fallback strategies.
11. **Resilience & Caching**: Add `Polly` for retries/circuit breaking and `Redis` for caching search results.
12. **Integration Tests**: Add `TestServer` backend integration tests.
13. **Advanced AI Integration**: Improve prompt engineering for more accurate intent extraction and result grounding, and/or implement native **Tool/Function Calling** with structured inputs/outputs to improve reliability over raw JSON parsing.
14. **Code Quality & Typing**: Audit the usage of `var` and explicit types throughout the solution to ensure that variables are typed as abstractions (interfaces) where appropriate for better decoupling and testability.
