# 002 - Project Standardization & Context Engineering

## Overview
This plan establishes the foundational development standards, documentation structure, and AI-assisted workflows for the "Find That Book" project. The goal is to ensure high code quality, standardized testing, and efficient context management for future features.

## Objectives
1.  **Centralized Documentation:** Establish `gemini.md` as the single source of truth for engineering standards.
2.  **Organized Planning:** Move feature plans to a dedicated directory with strict naming conventions.
3.  **Context Engineering:** Deploy custom prompts for specialized code reviews and security audits.
4.  **Testing Strategy:** Define strict coverage requirements (80% Unit) and tool selection (Playwright for E2E).

## Execution Steps

### 1. File Structure Reorganization
- [x] Create `docs/feature-plans/` directory.
- [x] Move old `PLAN.md` to `docs/feature-plans/001InitialBuildPLAN.md`.
- [x] Create `.gemini/` directory for prompt templates.

### 2. Core Documentation (`gemini.md`)
- [ ] Create `gemini.md` at root.
- [ ] Define **Testing Standards**:
    - Backend/Domain: xUnit + FluentAssertions (Min 80% coverage).
    - Frontend: Vitest + React Testing Library.
- [ ] Define **E2E Strategy**:
    - Tool: **Playwright**.
    - Scope: Critical User Journeys (Search, Results Display, Error Handling).
- [ ] Define **Naming Conventions**:
    - Plans: `[SequentialNumber][FeatureName]PLAN.md`.

### 3. Prompt Library Implementation
Create the following templates in `.gemini/`:
- [ ] `code-review-frontend.md`: Checklist for React/TS best practices, accessibility, and clean UI code.
- [ ] `code-review-backend.md`: Checklist for .NET Clean Architecture, performance, and API design.
- [ ] `security-audit.md`: OWASP Top 10 analysis prompt.

## Verification
- Confirm file structure matches the new convention.
- Verify `gemini.md` exists and contains defined standards.
- Verify `.gemini/` contains the 3 markdown prompts.
