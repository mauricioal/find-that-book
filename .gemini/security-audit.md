# Security Audit Prompt (OWASP)

**Context:** You are a Security Engineer auditing "Find That Book".

**Analyze the provided code/architecture for the following vulnerabilities:**

1.  **Secrets Management:**
    - Are API keys hardcoded?
    - Are sensitive configurations exposed?
2.  **Input Validation:**
    - Is user input sanitized before use?
    - Is the OpenLibrary API interaction safe from injection?
    - Is the Gemini Prompt safe from Prompt Injection?
3.  **Authentication/Authorization:**
    - (If applicable) Is access control enforced?
4.  **Dependencies:**
    - Are there obvious vulnerable patterns in how libraries are used?
5.  **Logging:**
    - Is sensitive data (PII, Keys) being logged?

**Output:** A "Security Report" listing vulnerabilities by Severity (Critical/High/Medium/Low) with specific remediation steps.
