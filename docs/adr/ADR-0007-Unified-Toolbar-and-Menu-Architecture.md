# ADR-0007: Unified Toolbar and Menu Architecture

## Status
Accepted

## Date
2026-05-09

---

## Context

Historically, toolbar and menu support in the WPF application framework evolved independently, resulting in:

- Divergent composition models (inline XAML vs. framework-owned structures)
- Duplicated command wiring between menus and toolbars
- Implicit or hidden behavior (especially for shortcuts)
- Limited testability and unclear ownership boundaries

As part of the BBApp.Next modernization effort, a deliberate, phased redesign was undertaken to unify toolbar and menu support under a single, command-centric, semantic architecture. This work was executed incrementally across:

- Toolbar Phases T1–T3 (Core semantics, WPF projection, TestHost validation)
- Menu Phases M1–M4 (Core semantics, WPF projection, TestHost validation, shortcut behavior)

The result is a coherent, layered system that treats toolbars, menus, and keyboard shortcuts as different *presentations of the same underlying intent*.

---

## Decision

We adopt a **unified semantic architecture for toolbars and menus** with the following principles:

1. **Commands are the single execution axis**
   - All user actions are represented by shared `ICommand` instances
   - Toolbars, menus, and keyboard shortcuts invoke the *same* command objects

2. **Semantic models live in Dev.Core**
   - Toolbars: `ToolbarItem`, `ToolbarItemKind`, semantic metadata
   - Menus: `MenuItem`, `MenuItemKind`, hierarchy, structured `MenuShortcut`
   - Core types are UI-agnostic, immutable-oriented, and unit-testable

3. **Projection lives in Dev.Wpf**
   - `ToolbarHostControl` and `MenuHostControl` render Core semantics into native WPF UI
   - No framework-owned ViewModels
   - No behavior hidden inside controls

4. **Behavior is owned at natural boundaries**
   - Mouse and invocation behavior flows through `ICommand`
   - Keyboard shortcut behavior is owned at the **Window level** via a dedicated shortcut host/behavior
   - Menus *describe* shortcuts; windows *execute* them

5. **Minimal consumer surface**
   - Applications compose toolbars and menus entirely in their ViewModels
   - Views bind with minimal XAML (typically a single host control)
   - No DataTemplates, converters, or policy logic required in consuming apps

---

## Consequences

### Positive

- Toolbars, menus, and shortcuts are fully unified and consistent
- Command duplication is eliminated
- Shortcut behavior is explicit, testable, and window-scoped
- Framework layering is clear and enforceable
- TestHost validation demonstrates real-world usability
- The system scales cleanly to BBApp.Next and future apps

### Trade-offs

- Shortcut conflict resolution and precedence are intentionally deferred
- Context menus and advanced shortcut policies remain future enhancements
- Some WinForms-era conveniences (implicit behavior) are no longer automatic

These trade-offs are intentional and preserve long-term architectural integrity.

---

## Alternatives Considered

1. **Menu-owned shortcut behavior**
   - Rejected due to violated separation of concerns and poor lifecycle control

2. **Global application-wide shortcut registry**
   - Rejected due to scoping, testing, and multi-window issues

3. **Unified toolbar/menu base type**
   - Rejected as premature abstraction; menus and toolbars differ structurally (hierarchy vs. flat)

---

## Result

The framework now provides a modern, compositional, and command-centric foundation for toolbars, menus, and keyboard shortcuts. This architecture is complete, validated, and suitable as the long-term standard for BBApp.Next and other WPF applications.

ADR-0007 formally records this decision and supersedes any prior implicit or ad-hoc toolbar/menu patterns.
