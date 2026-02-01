# 005 - Refine Matching Hierarchy & Normalization

## Overview
This plan refines the `BookMatcher` logic to strictly adhere to the user's detailed clarification of the Matching Hierarchy (Requirements 4a-4e).

## Key Changes
1.  **Strict Normalization:** Stop removing articles/stop-words. "Normalization" will strictly mean Case Insensitivity and Punctuation removal ONLY.
    *   *Implication:* "The Hobbit" != "Hobbit" (Exact Match fails).
2.  **Strict Ranking Logic (4a-4d):**
    *   **Rank A (Strongest):** Strict Title Match + Primary Author Match.
    *   **Rank B:** Strict Title Match + Contributor Match (Primary Author Mismatch).
    *   **Rank C:** Partial Title Match + Author Match (Primary OR Contributor).
    *   **Rank D:** Author Match Only (No Title Match).
3.  **Exclusive Top-Rank Return (4e):**
    *   The API must return candidates **ONLY from the highest achieved rank**.
    *   *Example:* If we find 2 books with Rank A, we return ONLY those 2. We do NOT return Rank B books to fill the quota up to 5.
    *   *Example:* If no Rank A, but 3 Rank B, we return only Rank B.

## Implementation Steps

### 1. Update `BookMatcher` (Logic Refinement)
*   **Remove `GetCoreTitle` logic** or modify it to stop removing "the", "a", "of".
*   **Normalization:** `Input.ToLower().RemovePunctuation()`.
*   **Refine Rank C:** Explicitly allow "Primary OR Contributor" for the author check in this tier.

### 2. Update `BookSearchService` (Filtering Logic)
*   **Current Logic:** Sorts by Rank and takes top 5.
*   **New Logic:**
    1.  Calculate Ranks for all candidates.
    2.  Find `MaxRank` present in the set.
    3.  Filter: `candidates.Where(c => c.Rank == MaxRank)`.
    4.  Take top 5 of *that specific rank only*.

### 3. Verification (Tests)
*   **Normalization Test:** Ensure "hobbit" is NOT an Exact Match for "The Hobbit".
*   **Filtering Test:** Mock a set with 1 StrongMatch and 4 NearMatches. Assert ONLY the 1 StrongMatch is returned.
*   **Rank C Test:** Verify "Partial Title + Contributor" lands in Rank C.
