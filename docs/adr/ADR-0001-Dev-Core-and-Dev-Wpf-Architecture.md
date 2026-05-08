# ADR-0001: Dev.Core and Dev.Wpf Framework Architecture

## Status
Accepted — May 2026

## Context

Dev.Core and Dev.Wpf originated within the BentleyBuildApp.Next repository to support a modern WPF application architecture targeting .NET 10+. Over time, these projects evolved into a reusable, application-agnostic UI framework providing shared infrastructure beyond the needs of a single application.

As usage expanded and additional applications (e.g., PersonalLedger) emerged as potential consumers, it became necessary to formally define the architectural intent, scope, and boundaries of Dev.Core and Dev.Wpf independently of any specific application.

This ADR captures the framework-specific architectural decisions originally documented as part of the BentleyBuildApp.Next project structure and re-homes them as first-class decisions owned by the Dev.Core / Dev.Wpf framework.

## Decision

Dev.Core and Dev.Wpf together form a **WPF Application Framework** with the following responsibilities and constraints:

### Dev.Core

Dev.Core is a platform-agnostic application-support library targeting `net10.0`.

It provides:
- Shared application services and service interfaces
- Application- and session-scoped non-UI logic
- View-model infrastructure intended for MVVM-style applications
- Cross-cutting UX-related models (toolbars, window persistence, version checks, application metadata)

Dev.Core MUST:
- Remain completely free of WPF or UI-specific dependencies
- Avoid static state
- Remain fully unit-testable without a UI runtime
- Act as a leaf project with no internal framework dependencies

Dev.Core MUST NOT:
- Reference WPF types
- Contain application-specific domain logic
- Encode persistence or platform mechanics directly

### Dev.Wpf

Dev.Wpf is a shared WPF UI infrastructure library targeting `net10.0-windows`.

It provides:
- Shared WPF controls and control templates
- Behaviors, converters, and styling infrastructure
- Theme management and application-level visual consistency
- Base dialogs and windows
- UI service implementations that adapt Dev.Core abstractions for WPF

Dev.Wpf MUST:
- Reference Dev.Core only
- Contain no application-specific workflows or domain logic
- Act as a reusable UI substrate for multiple applications

Dev.Wpf MUST NOT:
- Contain business or persistence policy
- Reference application-layer or domain-layer assemblies

### Reference Boundaries

The following dependency rules are enforced:

- Dev.Core references no other framework or application projects
- Dev.Wpf may reference Dev.Core
- Applications may reference Dev.Core and Dev.Wpf
- No reverse dependencies are permitted

## Rationale

- **Reusability**: Dev.Core and Dev.Wpf are designed to support multiple WPF desktop applications without modification.
- **Testability**: Dev.Core targets `net10.0` and is fully testable with NUnit without a WPF runtime.
- **Clean layering**: Separating non-UI infrastructure from WPF mechanics prevents leakage of UI concerns into application and domain layers.
- **Modern WPF practices**: The framework adheres to current WPF and MVVM standard operating practices without binding applications to a specific MVVM framework identity.
- **Longevity**: By encoding intent rather than implementation detail, the framework can evolve internally without forcing consumer changes.

## Consequences

### Positive
- Applications consume a clearly defined WPF application framework rather than ad-hoc shared code
- Architectural boundaries are explicit and enforceable
- Shared UX infrastructure (toolbars, modes, output, dialogs) evolves in one place
- The framework can be versioned, tested, and released independently

### Negative
- Applications must respect stricter dependency rules
- Cross-cutting framework changes require coordinated updates across consuming apps
- WPF-specific behavior testing requires additional infrastructure beyond standard NUnit

## Alternatives Considered

### Application-local shared code

Keeping Dev.Core / Dev.Wpf embedded within each application repository. Rejected due to duplication, unclear ownership, and long-term maintenance risk.

### Monolithic UI framework

Combining Dev.Core and Dev.Wpf into a single WPF-only library. Rejected to preserve platform-agnostic services and enable non-UI testing.

## Follow-Up Work

- Formalize additional framework subsystems (e.g., mode-based UX, output infrastructure, persistent prompt suppression) via dedicated ADRs
- Introduce WPF-specific test hosting when UI behavior testing becomes a priority
- Evaluate NuGet packaging once framework APIs stabilize

## Notes

This ADR is derived from and supersedes the Dev.Core / Dev.Wpf-specific portions of ADR-0001 in the BentleyBuildApp.Next repository. Application-specific structure and policies remain governed by their respective repositories.
