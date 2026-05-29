# ADR-0015: Mode-Based UX Subsystem (Framework-Level Design)

## Status
Accepted — April 8, 2026 (Updated May 29, 2026)

## Context

Originally developed to support advanced, stateful UI workflows in BentleyBuildApp.Next, the Mode-Based UX subsystem was later identified as a fully generic, reusable framework capability suitable for inclusion in Dev.Core.

Advanced workflows share common characteristics:
- Temporarily take ownership of primary UI surface
- Maintain transient state
- Require explicit lifecycle boundaries
- Support transactional semantics (Apply / Cancel)

The original implementation proved to be:
- Fully generic (no product-specific dependencies)
- Independent of any UI framework (no WPF requirement)
- Fully unit testable in a headless environment

These qualities motivated extraction into Dev.Core.

> ***NOTE:***
> Product-specific UX rules (such as output panel behavior and layout enforcement) have been moved to a BentleyBuildApp.Next-specific ADR to preserve framework neutrality.

## Decision

The Mode-Based UX subsystem is a framework-level capability located in `Dev.Core.Services.Mode`.

### Core Components

- `BaselineMode`
- `IFeatureMode`
- `IModeService`
- `ModeService`
- `BaselineModeChangedEventArgs`
- `FeatureModeChangedEventArgs`

### Key Behavioral Guarantees

- Single active feature mode at any time
- Explicit lifecycle hooks (`OnEnter`, `OnExit`)
- Transactional semantics:
  - `Apply` may fail without exiting
  - `Cancel` always exits without side effects

### Framework Constraints

- No UI dependencies
- No product-specific logic
- Shutdown blocking is *supported* but not *mandated*

### Placement Rules

- Dev.Core owns abstractions and default implementation
- UI layers consume `IModeService`
- Products implement `IFeatureMode`

## Rationale

- Enables reuse across multiple applications
- Maintains strict separation of concerns
- Ensures full testability without UI runtime
- Aligns with Dev.Core ownership principles

## Consequences

### Positive
- Clear architectural ownership
- Reusable across products
- Stable abstraction for feature development

### Negative
- Requires careful contract stability management

## References

- [ADR-0002: Toolbar Visibility System](ADR-0002-Toolbar-Visibility-System.md)
- [ADR-0001: Project Structure](ADR-0001-Dev-Core-and-Dev-Wpf-Architecture.md)
