# ADR-0012: Output System Architecture

## ADR Number
ADR-0012

## Title
Output System Architecture

## Status
Accepted

2026-04-06

## Context

BBApp.Next required a production-ready output system that is correct under real process streaming, testable across layers, and resilient to the failure modes observed in BBApp.One. In BBApp.One, UI-facing output handling blurred boundaries between process piping, output interpretation, and rendering, which made overwrite semantics fragile and reduced long-term maintainability.

This decision fits the existing project boundaries as follows:

- BentleyBuild owns output-domain behavior: event model, event channel, process-to-event bridge, and renderer translation.
- BentleyBuildApp owns WPF presentation concerns: output surface, dispatcher marshaling boundary, and focus/activation behavior.
- Dev.Core and Dev.Wpf remain infrastructure and do not become output-semantic authorities.

The architecture is aligned with .NET 10 constraints and BBApp.Next layering rules:

- No WPF dependencies in BentleyBuild.
- No dispatcher usage outside BentleyBuildApp.
- UI remains a consumer of rendered snapshots, not an interpreter of terminal behavior.

Implementation has been completed through phases O1–O5:

- O1: Output surface and activation contract.
- O2: Event model and thread-safe output channel.
- O3: Process execution capture to output events.
- O4: UI-agnostic renderer and viewport snapshots.
- O5: WPF binding with dispatcher boundary.

## Decision

BBApp.Next adopts an event-driven, layered output architecture with explicit separation between execution, event transport, rendering translation, and WPF presentation.

The final architecture is:

- Process execution layer emits output lifecycle and text as immutable output events.
- Output Event Channel provides ordered, thread-safe publication/subscription with no UI assumptions.
- Output Renderer translates ordered events into immutable viewport snapshots using minimal terminal-style semantics.
- Output View binding in BentleyBuildApp marshals at a single dispatcher boundary and renders snapshot text in a read-only, selectable output surface.

Key decisions:

1. Output is modeled as events, not raw append-only strings.
2. Lifecycle events (process start/exit) remain distinct from renderable content events.
3. Renderer logic remains entirely UI-agnostic and WPF-free.
4. Dispatcher usage is isolated to BentleyBuildApp boundary coordination.
5. Viewport state is exposed as snapshot-based immutable models.
6. The design preserves append and carriage-return overwrite semantics without full terminal emulation.

### Explicit Non-Goals

The output system intentionally does not provide:

- Full terminal emulation.
- ANSI escape sequence parsing or styling (current phase scope).
- UI-side interpretation of control characters.
- Execution logic inside WPF views.
- Persistence of output history.
- Parallel command execution support in the output architecture.

## Rationale

The core problem is faithful output behavior under asynchronous process streaming, not merely text display. Modeling output as events provides explicit meaning at the transport boundary and avoids coupling rendering semantics to process APIs or WPF controls.

Isolating the renderer from WPF ensures:

- Deterministic behavior testable in NUnit without UI infrastructure.
- Stable semantics for append/overwrite/reset independent of presentation framework.
- Reuse potential across future clients.

Restricting dispatcher usage to a single UI boundary coordinator ensures:

- No hidden threading assumptions inside execution, channel, or renderer layers.
- Predictable cross-thread behavior.
- Clear ownership of marshaling responsibilities.

Keeping lifecycle events separate from renderable text avoids conflating process state with viewport content and keeps extension points clean for future phases (for example, richer UX lifecycle indicators or ANSI interpretation layers) without changing execution contracts.

Snapshot-based viewport models were chosen because they provide a simple, immutable handoff contract from renderer to UI, favoring correctness and determinism over early optimization.

This architecture directly addresses BBApp.One issues by preventing UI-level ad-hoc interpretation of streamed text and by centralizing terminal-style behavior in a dedicated, testable translation layer.

## Alternatives Considered

### Alternative 1: Direct text append in WPF from process stdout/stderr callbacks
Rejected because it couples execution and rendering to UI controls, makes overwrite semantics fragile, and introduces thread-marshaling risk throughout the code path.

### Alternative 2: Keep an IOutputSink-style abstraction used for both transport and rendering
Rejected because it conflates responsibilities and repeats the boundary ambiguity seen in BBApp.One.

### Alternative 3: Interpret terminal semantics directly in OutputView
Rejected because it violates layering, reduces testability, and makes semantics dependent on WPF behavior.

### Alternative 4: Build a full terminal emulator immediately
Rejected as unnecessary complexity for current requirements and inconsistent with phased delivery goals.

## Consequences

### Positive

- Clear separation of concerns across execution, transport, translation, and presentation.
- Deterministic and testable behavior at each layer.
- Threading model is explicit and maintainable.
- Renderer semantics are reusable and independent of WPF.
- Single dispatcher boundary reduces cross-thread defect surface.
- Future ANSI support can be added without refactoring core execution/channel contracts.

### Negative

- More architectural types and coordination classes than a direct UI-append approach.
- Snapshot translation and text projection introduce additional allocation/translation cost.
- No immediate ANSI/color/styling support in current scope.
- Additional integration steps are required to wire execution UX behaviors end-to-end in later phases.

### Follow-Up Work

- Phase O6 integration and UX polishing on top of the adopted architecture.
- Optional Phase O7 ANSI/extended control support as an additive interpretation layer.
- Performance tuning only if profiling demonstrates snapshot translation overhead is material.

## References

- [BBApp.Next.Output.System.Design.md](../design/BBApp.Next.Output.System.Design.md)
- [BBApp.Next.Output.System.Phases.md](../design/BBApp.Next.Output.System.Phases.md)
- [ADR-Template-BBAppNext.md](ADR-Template-BBAppNext.md)
- [ADR-0011-Environment-First-Application-Flow.md](ADR-0011-Environment-First-Application-Flow.md)
