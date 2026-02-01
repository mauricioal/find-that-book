# 006 - Refine Author Resolution (Primary vs Contributor)

## Problem Analysis
OpenLibrary's `/works/{id}.json` often returns a list of authors where contributors (adaptors, illustrators) are mixed with the primary author, all sharing the same `author_role` type. 
Our current C# logic erroneously treats all listed authors as "Primary", leading to incorrect ranking (e.g., Charles Dixon being treated as a primary author for "The Hobbit").

## Goal
Use AI to correctly "Resolve Primary Authors" (Requirement 3b-ii) and "Extract Useful Information" (Requirement 2a), ensuring the matching hierarchy (Requirement 4) is applied correctly.

## Implementation Steps

### 1. Refactor `IOpenLibraryClient`
- Rename `GetPrimaryAuthorsAsync` to `GetWorkAuthorsAsync`.
- It will return all names found in the `works.authors` record.

### 2. Implement `IAiService.ResolveAuthorsAndRankAsync`
- Instead of just generating an explanation, the AI will now act as the **Resolution Engine**.
- **Input:** Raw Query, Book Candidates (with full author lists).
- **Task:** 
    1. Identify the **Primary Author(s)** for each book from the provided list based on general knowledge.
    2. Identify **Contributors** (adaptors, illustrators).
    3. Calculate the **MatchRank** (Strong, Contributor, Near, etc) based on the Hierarchy rules.
    4. Generate the **Explanation**.

### 3. Update `BookSearchService` (Orchestrator)
- Pass the "raw data" (Found authors) to the AI service.
- The AI service will return the "Ranked and Categorized" list.

### 4. Benefits
- **Requirement Compliance:** Directly addresses the instruction to "Use AI to extract useful information" and "resolve primary authors".
- **Fact Accuracy:** The AI knows Tolkien is the primary author of "The Hobbit", correctly identifying Dixon as a contributor even if the OpenLibrary JSON is ambiguous.
- **Cleaner Code:** Removes complex and potentially failing heuristics from the C# codebase.

## Verification
- Test case: "the hobbit dixon" -> Should rank as `TitleAndContributorMatch` (Rank 4) because Dixon is an adaptor, not primary.
- Test case: "the hobbit tolkien" -> Should rank as `StrongMatch` (Rank 5).
