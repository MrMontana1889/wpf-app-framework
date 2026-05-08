# ADR-0006: Do Not Prompt Again Messaging System (Dev.Core / Dev.Wpf)

## Status
Accepted — May 2026

## Context

Modern desktop applications frequently require user confirmation or acknowledgement for recoverable conditions such as warnings, confirmations, or potentially destructive actions. Repeatedly prompting users for the same decisions can lead to degraded usability, frustration, or habituation.

The original BentleyBuildApp (BBA1) implementation provided ad-hoc suppression of some prompts, but it was application-specific, difficult to extend, and not well aligned with modern architectural principles.

As part of the extraction of **Dev.Core** and **Dev.Wpf** into the reusable **wpf-app-framework**, a generic, application-agnostic solution for suppressible prompts is required. This ADR formalizes the architectural design of the **Do Not Prompt Again Messaging System** as framework infrastructure rather than application logic.

This ADR is derived from the authoritative BBApp.Next design document, with application-specific details removed and ownership boundaries clarified for framework use.

## Decision

The framework shall provide a **Do Not Prompt Again Messaging System** with the following characteristics:

- Generic and reusable across applications
- Explicit in behavior and lifecycle
- Decoupled from UI technology at the core layer
- Request/response oriented instead of boolean-driven
- Deterministic and fully testable

The system is divided across Dev.Core and Dev.Wpf with clearly defined responsibilities.

## Architecture

### Dev.Core Responsibilities

Dev.Core owns the **semantic and behavioral aspects** of prompt suppression. It shall provide:

- Stable prompt identity definitions
- Prompt definition and registry infrastructure
- Prompt request and response contracts
- Suppression lookup and persistence services
- Core orchestration logic

Dev.Core MUST:
- Contain no WPF or UI dependencies
- Contain no application-specific wording or prompt registration
- Be fully unit-testable without a UI runtime

---

### Dev.Wpf Responsibilities

Dev.Wpf owns the **WPF-specific presentation layer** for prompts. It shall provide:

- Dialog UI for prompts
- Button layouts and default focus behavior
- Suppression checkbox presentation and binding
- Result capture and translation to framework responses

Dev.Wpf MUST:
- Reference Dev.Core only
- Contain no application-specific logic
- Contain no prompt registration

---

### Application Responsibilities (Consumers)

Applications consuming the framework are responsible for:

- Defining and registering prompt definitions
- Choosing prompt wording, titles, and stable keys
- Interpreting prompt responses

Applications MUST NOT:
- Implement suppression persistence
- Infer or override suppression behavior
- Encode prompt semantics directly in UI code

## Prompt Identity

Each suppressible prompt is identified by a **stable, namespaced string key**.

Examples:

- Build.VisualStudioRunning
- Workspace.ResetConfirmation
- Environment.CustomStrategyDuplicate

Rules:
- Prompt identifiers must remain stable across application versions
- Identifiers must not depend on localized text or runtime parameters
- The identifier defines the scope of suppression

## Prompt Registry

A **Prompt Registry** serves as the authoritative catalog of known prompts.

Responsibilities:
- Register prompt definitions
- Enforce identifier uniqueness
- Provide lookup at runtime

Prompt definitions are registered once during application startup.

## Prompt Definition

A **PromptDefinition** describes the static metadata of a prompt.

Conceptual fields:
- PromptId
- TitleTemplate
- MessageTemplate
- ButtonSet
- DefaultResult
- AllowSuppression
- SuppressionText

Notes:
- Templates use standard .NET formatting tokens (`{0}`, `{1}`, …)
- Formatting occurs at request time, not registration
- `AllowSuppression` is the explicit and sole gate for suppression eligibility

## Prompt Request

A **PromptRequest** represents a single invocation of a prompt.

Conceptual fields:
- PromptId
- Parameters
- Optional presentation overrides (advanced scenarios only)

Overrides MAY change presentation text only. They MUST NOT affect:
- Prompt identity
- Suppression eligibility
- Button sets
- Default results

## Prompt Response

A **PromptResponse** captures the user’s decision and suppression intent.

Conceptual fields:
- Result
- SuppressChecked

Clarifications:
- Suppression is metadata, not behavior
- Callers interpret responses explicitly
- `Result.None` indicates a non-interactive outcome (suppression-based bypass)
- `Result.None` is never returned by UI dialogs
- `Result.None` is never persisted

## Suppression Service

A **Prompt Suppression Service** manages persistence and lookup of suppressed responses.

Responsibilities:
- Retrieve stored responses by prompt key
- Persist responses when suppression is selected
- Clear individual or all responses (future extension)

Persistence model:
- User-profile scoped
- Disk-backed
- Soft-fail with logging on load/save errors

Guarantees:
- Persistence failure does not affect the current prompt
- No suppression is assumed on failure
- No retries or fallback logic are performed

The service must be UI-agnostic and deterministic.

## Dialog Orchestration Flow

1. Caller creates a PromptRequest
2. Definition is resolved from the registry
3. If suppression is allowed, stored responses are consulted
4. If a stored response exists:
   - Dialog is skipped
   - Stored response is returned (`Result.None` if applicable)
5. Otherwise:
   - Dialog is displayed
   - User selects a concrete result
   - User may opt into suppression
6. If suppression is selected and the result is not Cancel/Close:
   - Response is persisted
7. Caller receives the PromptResponse

## UX Rules

- Suppression checkbox is shown only when `AllowSuppression` is true
- Cancel and Close actions are never persisted
- DefaultResult affects initial focus only
- Dialog remains open until a concrete result is chosen

## Non-Goals

The framework explicitly does not provide:

- Time-based expiration of suppressions
- Workspace- or document-scoped suppression
- Telemetry or analytics
- Localization strategy beyond templates
- Built-in management UI

## Consequences

### Positive

- Consistent, explicit prompt suppression behavior across applications
- No hidden or emergent UX rules
- Clean separation of behavior and presentation
- Fully testable core logic
- Future extensibility without breaking callers

### Negative

- Applications must explicitly register prompt definitions
- Slightly higher upfront design effort

## Summary

This ADR establishes the **Do Not Prompt Again Messaging System** as a first-class framework capability within Dev.Core and Dev.Wpf. By encoding suppression semantics explicitly and separating UI from behavior, the framework provides a durable, reusable foundation for recoverable user prompts across all consuming WPF applications.
