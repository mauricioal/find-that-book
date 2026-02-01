# Feature Plan: Fix Primary Author Logic

**Feature ID:** 005
**Feature Name:** FixPrimaryAuthorLogic
**Status:** Complete

## 1. Introduction
The current implementation of `GetPrimaryAuthorsAsync` in `OpenLibraryClient` does not correctly distinguish between primary authors and contributors (illustrators, editors, etc.). This leads to inaccurate matching where a contributor match might be ranked as highly as a primary author match. The goal is to refine the Open Library integration to correctly identify primary authors from the canonical work records.

## 2. Problem Description
- `GetPrimaryAuthorsAsync` currently fetches all authors listed in a work's `authors` array and treats them all as "primary".
- The Open Library Search API (`/search.json`) returns a flat `author_name` list that mixes all contributors.
- We need to use the Work API (`/works/{id}.json`) to inspect the `authors` list more carefully, looking for role/type distinctions to identify true primary authors.

## 3. Proposed Changes

### 3.1. Domain Entities
- **Update `BookCandidate`**:
    - Add `List<string> PrimaryAuthors` property.
    - Add `List<string> Contributors` property.
    - (Optional) Deprecate or repurpose the existing `Authors` property to be a union or specific fallback.

### 3.2. Infrastructure (`OpenLibraryClient`)
- **Update `OpenLibraryWork` DTO**:
    - Add a property to capture the `type` or `role` of the author entry in the `authors` list.
    - Example:
      ```csharp
      private class AuthorRole
      {
          [JsonPropertyName("author")]
          public AuthorKey? Author { get; set; }
          
          [JsonPropertyName("type")]
          public AuthorType? Type { get; set; } // To check for specific roles
      }
      ```
- **Update `GetPrimaryAuthorsAsync`**:
    - Rename to `GetWorkDetailsAsync` or `GetAuthorsAsync` to reflect it fetches more than just names.
    - Return a structured result (e.g., `(List<string> Primary, List<string> Contributors)`).
    - Logic:
        - Fetch work.
        - Iterate `authors`.
        - If `type` indicates a contributor (e.g., illustrator, editor) or if it's not the first author (depending on OL conventions found), categorize accordingly. *Note: Open Library often puts the primary author first. Explicit roles are sometimes used.* 
        - For this implementation, we will fetch the author details.
        - We will implement logic to prioritize the first author as primary if no explicit roles distinguish them, or inspect available role fields.

### 3.3. Application Service (`BookSearchService`)
- **Update `SearchAsync`**:
    - Instead of just `GetPrimaryAuthorsAsync`, call the updated method.
    - Populate `BookCandidate.PrimaryAuthors` and `BookCandidate.Contributors`.
    - Update the matching logic loop:
        - `isPrimary` should be true ONLY if the matched name is in `PrimaryAuthors`.
        - If the matched name is in `Contributors`, it is a lower signal match (`MatchMetadata` or `MatchRank` adjustment).

### 3.4. Matcher (`BookMatcher`)
- Ensure `CalculateMatch` respects the `isPrimary` flag correctly (it currently accepts it as a parameter, so logic might mainly be in `BookSearchService` passing the right bool).

## 4. Verification Plan
- **Unit Tests**:
    - Update `OpenLibraryClientTests` (if exists, or mock) to verify parsing of work authors.
    - Update `BookSearchServiceTests` to verify `isPrimary` is calculated correctly based on the new split.
- **Manual/Integration**:
    - Run the application.
    - Search for "The Hobbit" (often has illustrators).
    - Verify "J.R.R. Tolkien" is Primary.
    - Verify "Alan Lee" (if listed) is Contributor/lower rank.

## 5. Implementation Steps
1.  Modify `BookCandidate` class.
2.  Update `OpenLibraryClient` DTOs and Logic.
3.  Update `IOpenLibraryClient` interface.
4.  Update `BookSearchService` to use the new interface method and populate candidate lists.
5.  Run tests and verify.
