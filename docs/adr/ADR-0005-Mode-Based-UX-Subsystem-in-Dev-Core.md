# ADR-0015: Extract Mode-Based UX Subsystem to Dev.Core

## ADR Number
ADR-0015

## Title
Extract Mode-Based UX Subsystem to Dev.Core

## Status
Accepted — April 8, 2026

## Context

BBApp.Next requires support for advanced, stateful UI workflows — operations
that occupy the main content panel, require transient state management, and
must block application shutdown until they are resolved. Examples include
Filter Solution and future editor workflows.

To support this class of interaction, a **Mode-Based UX subsystem** was
designed and implemented across two phases:

**Phase U1** established the foundational lifecycle invariants:
- Two mutually exclusive baseline modes (`Simple` and `Advanced`) controlling
  the default UI posture.
- A single-active feature mode abstraction that owns the main content panel
  while active, exposes an optional primary toolbar, and enforces application
  shutdown blocking during the session.
- Explicit enter / exit lifecycle hooks (`OnEnter`, `OnExit`) with guaranteed
  event ordering.
- Output panel visibility guaranteed unconditionally while any feature mode
  is active.

**Phase U2** added transactional semantics:
- Feature modes may attempt `Apply` (commit staged state); a failed apply
  leaves the mode active with no change to baseline state.
- `Cancel` unconditionally discards staged state and exits the mode.
- The shell responds correctly to each outcome without knowledge of the
  feature's business logic.

The subsystem was initially implemented inside `BentleyBuild.Services.Mode`.
At the completion of Phase U2, the following characteristics made early
extraction appropriate:

- The implementation is **fully generic**: it references only `ToolbarModel`
  from Dev.Core and has no dependency on BentleyBuild domain types.
- There are **no UI (WPF) dependencies**; all types target `net10.0`.
- The subsystem is **fully unit tested** (52 NUnit tests, no WPF runtime
  required).
- **No feature consumers existed** at the time of extraction, making the
  migration zero-risk and zero-cost in terms of consumer updates.

This ADR documents the decision to extract the subsystem into Dev.Core before
any feature implementation takes a dependency on it.

## Decision

The Mode-Based UX subsystem is a **framework-level capability** and resides
entirely in Dev.Core under `Dev.Core.Services.Mode`.

The subsystem comprises:

| Type | Role |
|---|---|
| `BaselineMode` | Enum: `Simple` / `Advanced` baseline postures |
| `IFeatureMode` | Feature mode abstraction: lifecycle hooks + transactional methods |
| `IModeService` | Service contract: mode lifecycle, shutdown guard, events |
| `ModeService` | Default implementation of `IModeService` |
| `BaselineModeChangedEventArgs` | Event data for baseline mode transitions |
| `FeatureModeChangedEventArgs` | Event data for feature mode enter / exit |

**Placement rules:**

- **Dev.Core** owns all mode contracts and the default implementation.
  No product-specific logic may reside here.
- **Dev.Wpf** may consume `IModeService` to govern layout switching and
  toolbar visibility but must not define mode rules.
- **BentleyBuild / BentleyBuildApp** implement `IFeatureMode` for
  product-specific features (e.g., Filter Solution) and consume `IModeService`
  via dependency injection.
- Feature implementations are **consumers** of the subsystem, not owners.
  Mode enforcement rules are the sole responsibility of `ModeService`.

## Rationale

**Reusability across future products.** Dev.Core is the shared platform layer
for all BBApp.Next-derived products. Placing mode infrastructure here ensures
that any future product — WPF, WebView, or Electron host — can adopt the same
lifecycle model without referencing BentleyBuild-specific assemblies.

**Testability without a UI runtime.** Because the subsystem targets `net10.0`
(not `net10.0-windows`) and has no WPF dependency, all 52 NUnit tests execute
in a standard test runner with no dispatcher or UI infrastructure. This mirrors
the strategy established in ADR-0003 for the Toolbar Visibility System.

**Separation of concerns.** The shell (BentleyBuildApp) enforces structural
rules — at most one feature mode, shutdown blocking, output visibility — while
individual features implement transient business logic. This boundary prevents
features from encoding shell policy and prevents the shell from encoding
feature-specific behaviour.

**Alignment with prior framework-level decisions.** ADR-0003 established
`IToolbarRegistryService` and `ToolbarModel` in Dev.Core. The Mode subsystem
complements this by governing which toolbars are visible and active. Placing
both in the same layer ensures coherent framework ownership.

**Zero consumer impact at extraction time.** No BentleyBuild feature had
taken a dependency on the mode types before this extraction. Migrating at this
point required only a namespace change and carried no consumer update cost.

## Alternatives Considered

### 1. Keep the subsystem in BentleyBuild

The subsystem could remain in `BentleyBuild.Services.Mode` as product-layer
infrastructure.

**Rejected** because:
- No BentleyBuild domain concept is referenced by the subsystem; it is fully
  generic.
- Future products would need to either copy the implementation or take a
  dependency on a product assembly, both of which violate Dev.Core's purpose.
- Keeping it in BentleyBuild would contradict the architectural principle that
  framework-level capabilities reside in Dev.Core (established in ADR-0001 and
  reinforced in ADR-0003).

### 2. Extract after the first feature consumer is implemented

Defer extraction until Filter Solution (or another feature) implements
`IFeatureMode`, and migrate at that point.

**Rejected** because:
- Migrating after consumers exist requires updating using statements, project
  references, and potentially DI registrations across multiple files
  simultaneously.
- The subsystem was already clean, generic, and fully tested — the optimal
  extraction moment had arrived.
- Deferral would embed a known framework concern into a product layer
  unnecessarily, creating technical debt with no offsetting benefit.

## Consequences

### Positive

- **Clear framework responsibility.** Dev.Core is the unambiguous owner of
  mode lifecycle contracts. New developers and AI coding agents have a single
  authoritative location to reference.
- **Easier feature development.** Feature mode authors implement one interface
  (`IFeatureMode`) against a stable Dev.Core contract; no knowledge of shell
  internals is required.
- **Safe transactional workflows.** The Apply / Cancel contract ensures
  baseline application state is never modified by a failed or cancelled feature
  operation, regardless of feature implementation.
- **Foundation for advanced mode-based UX.** Future phases (e.g., Phase U3:
  Filter Solution integration) have a stable, tested substrate to build on.
- **No WPF requirement for testing.** All mode logic is exercisable in
  headless unit tests, supporting CI pipelines without a Windows UI runtime.

### Negative

- **Up-front extraction cost.** Minor effort was required to migrate files,
  update namespaces, and recreate test infrastructure in Dev.Core.Tests.
- **Framework contract maintenance.** `IModeService` and `IFeatureMode` are
  now stable public contracts in a shared library. Breaking changes require
  coordinated updates across all consumers.

### Follow-Up Work

- **Phase U3:** Implement Filter Solution as the first `IFeatureMode`
  consumer, validating the subsystem end-to-end in BentleyBuildApp.
- No additional refactoring is required as a direct consequence of this ADR.

## References

- [ADR-0003-Toolbar-Visibility-System.md](ADR-0003-Toolbar-Visibility-System.md) —
  Established the precedent for placing shared UI infrastructure in Dev.Core;
  `ToolbarModel` is referenced by `IFeatureMode.PrimaryToolbar`.
- [ADR-0001-Project-Structure.md](ADR-0001-Project-Structure.md) —
  Defines the project boundary rules this ADR respects.
- `src/Dev.Core/Services/Mode/` — Production implementation.
- `tests/Dev.Core.Tests/Services/Mode/ModeServiceTests.cs` — 52 NUnit tests
  covering Phase U1 and Phase U2 behavioural contracts.
