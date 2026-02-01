# 003 - Strict Requirements Compliance Plan

## Overview
We identified a deviation from the "Functionality" section of the Assessment PDF. Specifically, we optimized away the **AI-based Explanation and Re-ranking** (Requirement 2b, 2c) in favor of deterministic C# logic. While robust, this technically violates the instruction to "Use AI to... produce explanations".

Additionally, we must ensure strict adherence to the **Matching Hierarchy** (Requirement 4), specifically distinguishing between **Exact/Normalized Matches** and **Near Matches**.

## Requirements Mapping
| Requirement | Current Status | Corrective Action |
| :--- | :--- | :--- |
| **2b. Use AI for explanations** | Replaced with C# strings | **Restore AI Step:** Feed C# verification results to AI to generate natural language explanations. |
| **2c. Optional AI Re-ranking** | Removed | **Restore AI Step:** Allow AI to adjust the final order of the top 5 (already filtered by C#) if needed. |
| **3b. Normalize Inputs** | Basic implementation | **Enhance Normalization:** rigorous lowercase/punctuation removal to detect "Exact Matches" correctly. |
| **4. Matching Hierarchy** | Implemented but needs refinement | **Strict Distinction:** Explicitly categorize "Exact Normalized" vs "Near/Partial" to drive Rank and Explanation. |

## Implementation Steps

### 1. Update Domain Entity (`BookCandidate`)
Add technical metadata fields to pass as "System Hints" to the LLM (not shown to user, but used for prompt context).
- `MatchType`: Enum/String (`ExactTitle`, `NearMatchTitle`, `AuthorFallback`)
- `AuthorStatus`: Enum/String (`Primary`, `Contributor`, `Unknown`)

### 2. Refine `BookMatcher` (The "Judge")
Update logic to strictly follow Requirement 4a-d:
- **Normalization:** Ensure "The Hobbit" and "the hobbit..." match as `Exact` after normalization.
- **Logic:**
    - **Rank A (Strongest):** Exact/Normalized Title + Primary Author.
    - **Rank B:** Exact/Normalized Title + Contributor.
    - **Rank C:** Near-match Title (Partial/Fuzzy) + Author.
    - **Rank D:** Author-only fallback.
- **Output:** Populate `MatchType` and `AuthorStatus` instead of a final user string.

### 3. Restore & Enhance `GeminiAiService.RankAndExplainResultsAsync`
- **Input:** List of `BookCandidates` with their calculated `MatchType` and `AuthorStatus`.
- **System Prompt:** 
    - "You are a librarian helper. I will provide a list of books and how they matched technically."
    - "If MatchType is 'ExactTitle', your explanation must state the title matched exactly."
    - "If MatchType is 'NearMatchTitle', state that the title was similar or partial."
    - "Use the AuthorStatus to mention if they are the primary author or a contributor (illustrator, etc)."
- **Goal:** The AI generates the *natural language* explanation, but it is **grounded** in the strict C# logic.

### 4. Update `BookSearchService` (Orchestrator)
- **Sequence:**
    1.  Extract Intent (AI).
    2.  Search OpenLibrary.
    3.  **Verify & Pre-rank (C#):** Apply strict hierarchy to determine the "Technical Truth".
    4.  **Final Polish (AI):** Call `RankAndExplainResultsAsync` with the top 5 pre-ranked items.

### 5. Verification
- **Test Case 1 (Exact):** "tolkien hobbit" -> Explains "Exact title match".
- **Test Case 2 (Near):** "tolkien silmaril" -> Explains "Title partially matches".
- **Test Case 3 (Contributor):** Query for an illustrator's book -> Explains they are a contributor.
