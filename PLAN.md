# Find That Book - Implementation Plan

## Overview
"Find That Book" is a library discovery service that takes messy, unstructured user queries (titles, authors, keywords) and uses Google Gemini to "understand" the intent, orchestrate searches against the OpenLibrary API, and return explained, ranked results.

## Architecture: Clean Architecture (Monolithic Project Structure)
We will follow the user's preferred structure, keeping layers distinct to ensure testability and separation of concerns.

### Project Structure: `FindThatBook.Api`
*   **`Domain`**: The core. Contains enterprise logic and types independent of external tools.
    *   `Entities`: Core data structures (`Book`, `SearchIntent`, `CandidateResult`).
    *   `Interfaces`: Abstractions for logic if strictly domain-bound (though typically External Service interfaces live in Application).
*   **`Application`**: Application business logic (Use Cases).
    *   `Interfaces`: `IOpenLibraryClient`, `IAiService`.
    *   `Services`: `BookSearchOrchestrator` (The main flow: Extract -> Search -> Rank -> Explain).
    *   `DTOs`: Data Transfer Objects for the UI.
*   **`Infrastructure`**: Implementation of external concerns.
    *   `ExternalTools`: `OpenLibraryClient` (HttpClient implementation).
    *   `AiProviders`: Configuration of `Microsoft.Extensions.AI` and `Gemini` client.
*   **`Web`**: Entry point.
    *   `Controllers`: `SearchController`.
    *   `Configuration`: DI setup, Middleware.

## Tech Stack
*   **Framework**: .NET 8 Web API
*   **AI Integration**: `Microsoft.Extensions.AI` (Abstractions) + `Google.Cloud.AIPlatform` or specialized community adapters (`GeminiDotnet.Extensions.AI` if available/preferred, or direct REST/GRPC integration wrapped in the abstraction).
*   **External Data**: OpenLibrary API.
*   **Testing**: xUnit, Moq (or NSubstitute), FluentAssertions.

## Implementation Roadmap

### Phase 1: Foundation & AI "Hello World"
*   [x] Initialize .NET 8 Web API solution and project structure.
*   [x] Configure `Microsoft.Extensions.AI` with Google Gemini.
*   [x] Implement `IAiService` to prove we can send a "messy string" and get a structured `SearchIntent` (JSON) back from Gemini.
*   [x] **Milestone**: Endpoint `/api/debug/extract` returns structured data from text (Implemented as part of main search flow).

### Phase 2: OpenLibrary Integration (The Tool)
*   [x] Define `IOpenLibraryClient` interface.
*   [x] Implement `OpenLibraryClient` in Infrastructure using `HttpClient`.
*   [x] Implement `search.json` fetching.
*   [x] **Milestone**: Endpoint `/api/debug/search` returns raw OpenLibrary results for a structured query.

### Phase 3: The Orchestrator (Core Logic)
*   [x] Implement `BookSearchService` in Application layer.
*   [x] Logic:
    1.  Receive raw string.
    2.  Call AI to extract search parameters.
    3.  Call OpenLibrary with parameters.
    4.  (Initial) Return raw results.
*   [x] **Milestone**: Functional end-to-end search (without deep ranking/explanation).

### Phase 4: Intelligence & Refinement (The "Why")
*   [x] Enhance AI integration to perform "Re-ranking" and "Explanation".
*   [x] Implement the specific matching hierarchy (Exact title > Title+Author, etc.) as defined in requirements.
*   [x] Handle edge cases (No results, ambiguous queries).

### Phase 5: Polish & Final Review
*   [x] Add comprehensive Unit Tests for the Orchestrator and Parsers (Core ranking logic tested).
*   [x] Ensure `README.md` documents setup and key choices.
*   [x] Final code cleanup and strict architectural review.

### Future Phase: Web UI
*   [ ] React App `FindThatBook.Web`.
