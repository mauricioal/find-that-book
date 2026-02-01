# 004 - Fix Strict Ranking Logic (Typos vs Exact Matches)

## Problem Analysis
The system misclassifies typos and partial keyword matches as "Exact Matches" because it relies on the LLM's corrected intent rather than the user's raw input.

## Goal
Enforce strict "Exact Match" rules by validating the **Raw Query** against the **Candidate Title**.

## Definitions (Strict)
*   **Exact Match:** The user's input (normalized) contains the **full canonical title** (minus articles/punctuation).
    *   "the hobbit" -> "The Hobbit" (✅ Exact)
    *   "hobbit" -> "The Hobbit" (✅ Exact - Core title present)
*   **Near Match:** The user's input contains *tokens* of the title, or is a typo.
    *   "hobiit" -> "The Hobbit" (✅ Near - Typo)
    *   "huckleberry" -> "The Adventures of Huckleberry Finn" (✅ Near - Partial/Keyword match)

## Implementation Steps

### 1. Update `IBookMatcher` Signature
Pass the `rawQuery` to the matcher.
```csharp
MatchResult CalculateMatch(string rawQuery, SearchIntent intent, BookCandidate candidate, bool isPrimaryAuthor);
```

### 2. Refine `BookMatcher` Verification Logic
Implement a `VerifyTitlePresence` check.

*   **Logic:**
    1.  `candidateCoreTitle` = Remove stop words ("the", "a", "of") from `Candidate.Title`.
    2.  `rawInput` = Normalize user input.
    3.  **Check:** Does `rawInput` contain `candidateCoreTitle`?
        *   **Yes:** Candidate for `ExactTitle`.
        *   **No:** Downgrade to `NearMatchTitle`.

*   **Examples:**
    *   Book: "The Hobbit" -> Core: "hobbit". Input: "hobiit". Contains "hobbit"? **No.** -> `NearMatch`.
    *   Book: "The Hobbit" -> Core: "hobbit". Input: "tolkien hobbit". Contains "hobbit"? **Yes.** -> `Exact`.
    *   Book: "Adventures of Huckleberry Finn" -> Core: "adventures huckleberry finn". Input: "huckleberry". Contains Core? **No.** -> `NearMatch`.

### 3. Update Orchestrator
Pass `rawQuery`.

### 4. Verification
Update tests to enforce this stricter logic.
