# Tech Stack & Versioning

This file tracks the specific versions of libraries and frameworks used in the project to ensure AI-generated code is compatible.

## Backend (.NET 8)
- **Runtime:** .NET 8.0
- **Framework:** ASP.NET Core Web API
- **AI Abstractions:** `Microsoft.Extensions.AI.Abstractions` (v10.2.0)
- **AI Implementation:** `GeminiDotnet.Extensions.AI` (v0.21.0)
- **Documentation:** `Swashbuckle.AspNetCore` (v6.6.2)
- **Testing:** `xUnit` (v2.9.0), `Moq` (v4.20), `FluentAssertions` (v8.8.0)

## Frontend (React)
- **Build Tool:** Vite (v7.3)
- **Runtime:** React (v19.0)
- **Language:** TypeScript (v5.7)
- **UI Framework:** Bootstrap (v5.3) via `react-bootstrap`
- **Icons:** `lucide-react`
- **Testing:** Vitest + React Testing Library (To be implemented)

## API Integrations
- **Google Gemini:** `gemini-1.5-flash`
- **OpenLibrary:** Search API (v1)
