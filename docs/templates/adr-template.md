# Architecture Decision Record (ADR)

## ADR Number
Sequential identifier (e.g., ADR-0001)

---

## Title
A short, clear, and descriptive title of the decision.

---

## Status

- Proposed
- Accepted
- Rejected
- Deprecated
- Superseded

Include date when status is set or changed.

Example:
- Accepted — May 2026

---

## Context

Describe the background and forces influencing this decision.

Include:

- The problem being solved
- Why this decision is needed now
- Relevant system context (where this applies)
- Constraints (technical, architectural, organizational)
- Any important assumptions

Optional considerations:
- Platform/runtime constraints (e.g., .NET version)
- Layering rules (e.g., UI vs non-UI)
- Reuse expectations
- Dependencies or external limitations

---

## Decision

State the decision clearly and precisely.

This should describe:
- What has been decided
- The architectural direction or rule being established

Where appropriate, include structured details such as:
- Responsibilities of components
- Constraints and requirements
- Allowed and disallowed behaviors

Guidelines:
- Be explicit
- Use bullet points when helpful
- Avoid ambiguity

---

## Rationale

Explain **why this decision was made**.

Consider:

- Maintainability
- Testability
- Architectural alignment
- Reusability
- Long-term sustainability
- Trade-offs accepted

This section should answer:
> “Why is this the right decision for this system?”

---

## Alternatives Considered

List meaningful alternatives that were evaluated.

For each alternative:
- Provide a brief description
- Include key pros and cons
- Explain why it was not selected

Avoid:
- Listing trivial or unrealistic options

---

## Consequences

Describe the outcomes of this decision.

### Positive

- Benefits gained
- Improvements to architecture or workflow
- Long-term advantages

---

### Negative

- Trade-offs or limitations introduced
- Increased complexity (if any)
- Constraints imposed on future work

---

## Follow-Up Work

List any work required to fully realize this decision.

Examples:
- Additional implementation phases
- Future ADRs
- Refactoring or migration tasks
- Testing or infrastructure improvements

---

## Constraints and Rules (Optional)

If the decision introduces enforceable rules, document them explicitly.

Examples:
- Dependency restrictions
- Layering rules
- Prohibited patterns

Use clear language such as:
- MUST
- MUST NOT
- SHOULD

---

## References

Include links to relevant supporting materials:

- ADRs → `docs/adr`
- Design documents → `docs/design`
- Related discussions, PRs, or issues

Examples:
- `docs/adr/adr-012-session-management.md`
- `docs/design/workspace-lifecycle.md`

⚠️ Guidelines:
- Do NOT restate documents
- Link only what is relevant
- Prefer specific references over general folders

---

## Notes (Optional)

Additional context that does not fit neatly into other sections.

Examples:
- Historical context
- Migration notes
- Relationship to other systems or repos

---

## Summary

Provide a concise recap of:

- The decision made
- Its purpose
- Its expected impact

This provides a concise summary of the decision and its impact.