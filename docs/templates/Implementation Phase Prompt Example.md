# Implementation Phase Prompt Example

## Objective
Implement the ability to load an existing workspace into the application, allowing users to reopen previously saved work and resume activity.

This phase introduces the foundational logic for loading persisted workspace data and integrating it into the running application session.

---

## Context
This phase builds upon the existing workspace creation functionality, which currently supports only new workspace initialization.

The system already includes:
- Workspace data models
- Save functionality (serialization)
- Basic application lifecycle management

This phase focuses on enabling the complementary "load" capability.

### 📚 References

- Design Doc → `docs/design/workspace-management.md`
- ADR → `docs/adr/adr-005-workspace-lifecycle.md`

These documents define:
- Workspace structure and lifecycle expectations
- Serialization/deserialization boundaries
- How the application should manage active workspace state

⚠️ Do not restate these documents — use them to guide implementation decisions.

---

## Scope

Implement functionality to:
- Load a workspace from an existing persisted file
- Deserialize the workspace data into application memory
- Initialize the application state using the loaded workspace
- Ensure the loaded workspace integrates with existing services and lifecycle management

Changes may involve:
- Application/core services responsible for workspace management
- File handling and deserialization logic
- Integration points with existing state management

---

### Non-Goals

- Do NOT implement UI for file selection (assume file path is provided)
- Do NOT modify save functionality unless required for compatibility
- Do NOT introduce new persistence formats
- Do NOT redesign workspace data structures

---

## Guidance (Optional)

- Ensure this logic resides in appropriate non-UI layers (e.g., core/application services)
- Avoid placing file or deserialization logic in UI components
- Reuse existing serialization mechanisms where possible
- Follow established patterns used in workspace creation

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

- Add tests validating successful workspace loading
- Add tests verifying correct handling of invalid or missing files
- Update any existing workspace-related tests impacted by this change

Tests should:
- Validate actual behavior of loading and initialization
- Cover edge cases such as corrupted data or missing fields
- Avoid trivial assertions

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

Stop once:
- Workspace loading functionality is fully implemented
- Integration with application state is complete
- All required unit tests are implemented and passing

Do not proceed into UI integration or additional features.

---

## Completion Summary Requirements (MANDATORY)

Cai must provide a summary including:

- What was implemented
- Files created or modified
- Key implementation decisions
- Any assumptions made (e.g., file structure expectations)
- Any follow-up considerations (e.g., UI integration, validation improvements)

---

## Notes

- This example demonstrates proper structure, tone, and level of detail
- Follow this pattern closely when creating new prompts
- Maintain flexibility — do not over-constrain implementation unless necessary
