# Implementation Phase Prompt Template

## Objective
Provide a high-level description of:
- What is being implemented
- Why it exists
- How it contributes to the overall system

---

## Context
Describe relevant background information:
- Where this fits within the system
- Any dependencies on prior phases
- Important architectural considerations

### 📚 References (If Applicable)
List any relevant documentation:

- ADRs → `docs/adr`
- Design Docs → `docs/design`

Examples:
- `docs/adr/adr-XXX-title.md`
- `docs/design/feature-name.md`

⚠️ Notes:
- Do NOT restate documents
- Use them to guide implementation decisions
- ADRs are assumed **authoritative and internally consistent**

---

## Scope

Clearly define what should be implemented.

Focus on:
- Functional expectations
- Affected components or layers
- Behavioral outcomes

Avoid:
- Overly prescriptive implementation details
- Forcing specific class or method names (unless absolutely necessary)

---

### Non-Goals

Explicitly define what is **out of scope**.

This prevents:
- Scope creep
- Over-engineering
- Premature abstraction

---

## Guidance (Optional)

Include only if ambiguity exists.

Examples:
- Project/layer placement guidance
- UI vs non-UI constraints

Guidelines for implementation:
- Prefer existing patterns in the codebase
- Reuse existing abstractions when appropriate
- Avoid introducing new patterns unless clearly justified

---

## Constraints (MANDATORY)

Cai must follow these rules:

- Do NOT create branches
- Do NOT commit code
- Do NOT push changes

All work must:
- Remain local
- Be reviewed by a human before commit

Additionally:
- Avoid unnecessary refactoring outside defined scope
- Ensure changes are safe and incremental

---

## Unit Test Requirements (MANDATORY)

Cai must:

- Add new tests where appropriate
- Update existing tests if behavior changes

Tests should:
- Validate behavior (not just execution)
- Include meaningful scenarios and edge cases
- Avoid trivial or redundant assertions

---

## Validation Requirements (MANDATORY)

Before completion, Cai must:

1. Build the **entire solution**
2. Run **all unit tests**

If ANY test fails:
- Fix immediately
- Repeat until all tests pass

No partial completion is allowed.

---

## ⛔ STOP Condition (CRITICAL)

Define a concrete stopping point.

Examples:
- "Stop once the required functionality and corresponding unit tests are complete."
- "Do not proceed beyond the described scope."

Purpose:
- Prevent scope creep
- Ensure controlled execution

---

## Completion Summary Requirements (MANDATORY)

Cai must provide a human-readable summary including:

- What was implemented
- Files created or modified
- Key design notes
- Any assumptions made
- Any follow-up considerations (if applicable)

---

## Notes for Usage

- This template must be followed **exactly in structure and order**
- Sections should not be rearranged or omitted
- Guidance should only be included when necessary
- Maintain flexibility — avoid over-specifying implementation details