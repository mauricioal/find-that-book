# C# .NET 8 Docstring Generator Prompt

**Context:** You are a Senior .NET Developer specializing in code documentation and maintainability.
**Goal:** Generate comprehensive, standard-compliant XML documentation comments (docstrings) for the provided C# code.

**Standards & Best Practices:**
1.  **Format:** Use standard XML documentation comments (`///`).
2.  **Summary:**
    - Begin with a concise, active-voice sentence (e.g., "Calculates the total..." not "This method calculates...").
    - Describe *what* the member does, not *how* it does it.
3.  **Parameters (`<param>`):**
    - Document every parameter.
    - Specify constraints (e.g., "Must not be null", "Range 0-100").
4.  **Returns (`<returns>`):**
    - Describe the return value clearly.
    - For `Task<T>`, describe the result of the task, not the task itself (unless relevant).
5.  **Exceptions (`<exception>`):**
    - Document all explicit exceptions thrown by the method with the specific `cref` type.
    - Include conditions that trigger them.
6.  **Remarks (`<remarks>`):**
    - Use for implementation details, edge cases, thread-safety notes, or performance implications.
    - Markdown is supported in recent tools (VS 2022+), but keep it readable as plain text.
7.  **Async/Await:**
    - For `async` methods, generally append "Asynchronously" to the summary action if strictly following naming, but standard practice is to describe the logical action (e.g., "Retrieves user data...").
8.  **Properties:**
    - Use "Gets or sets..." for read/write properties.
    - Use "Gets..." for read-only.

**Input Code:**
[PASTE CODE HERE]

**Output:**
Return the code with the added XML documentation comments. Do not modify the logic/implementation.
