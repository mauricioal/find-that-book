# Find That Book - Project Context & Standards

This file defines the engineering standards, architectural rules, and context for the "Find That Book" project. AI agents must strictly adhere to these definitions.

## 1. Architecture Overview
- **Style:** Clean Architecture (Monolithic Repository).
- **Backend:** .NET 8 Web API (`FindThatBook.Api`).
- **Frontend:** React + TypeScript + Vite (`FindThatBook.Web`).
- **Data:** OpenLibrary API (External).
- **AI:** Google Gemini via `Microsoft.Extensions.AI`.

## 2. Testing Standards
### Unit Testing
- **Requirement:** Mandatory for all Business Logic (Domain & Application layers).
- **Target Coverage:** Minimum **80%**.
- **Tools:**
    - Backend: `xUnit`, `FluentAssertions`, `Moq`.
    - Frontend: `Vitest`, `React Testing Library`.

### End-to-End (E2E) Testing
- **Requirement:** Critical paths must be covered (Search flow, Result rendering, Error states).
- **Tool:** **Playwright**.
- **Location:** `FindThatBook.E2E` (To be created).

## 3. Feature Planning
All major changes require a documented plan in `docs/feature-plans/`.
- **Naming Convention:** `[SequentialNumber][FeatureName]PLAN.md`
- **Example:** `002UserAuthPLAN.md`, `003SearchHistoryPLAN.md`.
- **Process:**
    1. Create plan.
    2. Approve plan.
    3. Implement.
    4. Mark as complete.

## 4. Code Quality Guidelines
- **Backend:**
    - Follow standard C# naming conventions (PascalCase for public, camelCase for private fields `_field`).
    - Use Dependency Injection for all services.
    - No business logic in Controllers.
- **Frontend:**
    - Functional Components only (Hooks).
    - Strict TypeScript typing (avoid `any`).
    - Use Bootstrap classes for layout; avoid inline styles.

## 5. Security (OWASP)
- **Injection:** Always use parameter binding (handled by .NET/EF Core).
- **Secrets:** NEVER commit API Keys. Use `appsettings.json` (gitignored) or Environment Variables.
- **XSS:** React auto-escapes, but validate all `dangerouslySetInnerHTML` usage.
