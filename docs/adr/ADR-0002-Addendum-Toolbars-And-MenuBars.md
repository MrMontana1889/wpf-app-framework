# ADR-0002 Addendum: Toolbars and Menu Bars as Semantic Rows

*Status: Accepted (Addendum)*
*Applies to: wpf-app-framework*

## Purpose

This addendum records the conclusions and clarifications reached during the completed toolbar and menu bar migration. It does **not** replace ADR-0002; instead, it captures the practical refinements and invariants that emerged from real-world implementation and validation.

The goal is to ensure the architectural intent of ADR-0002 remains stable, teachable, and non-ambiguous for future work, including BBApp.Next.

---

## Summary of Original ADR-0002

ADR-0002 established the foundational model that:

- **Semantic state** (existence, identity, visibility) is owned by the application
- **Projection** (UI rendering, filtering, layout) is owned by the framework
- Toolbars are defined semantically and projected by host controls
- Visibility is registry-driven and persists independently of UI lifetimes

This addendum builds on those principles.

---

## Menu Bars Are First-Class Semantic Rows

During migration it became clear that Menu Bars are best modeled as **semantic rows**, equivalent to toolbars for visibility purposes.

Key clarification:

- A Menu Bar is **not a special case**
- It participates in the same visibility system as toolbars
- It is shown or hidden based on semantic registry state

Treating Menu Bars as semantic rows ensures:

- Consistent customization behavior
- Persistence across application restarts
- Cross-platform projection compatibility

---

## Unified Visibility Model

Visibility for both toolbars and menu bars is governed exclusively by `IToolbarRegistryService`:

- State is keyed by `ToolbarId`
- Projection controls query and observe registry state
- Visibility must survive:
  - Toolbar item rebuilds
  - ItemsSource mutation
  - Control recreation

**Controls must not own or duplicate visibility state.**

---

## Projection Responsibilities

The following projection responsibilities are now explicitly defined:

### ToolbarHostControl

Owns:

- Projection of toolbar rows
- Filtering of toolbar items based on registry state
- Host-level toolbar context menu
- Right-click handling on:
  - toolbar items
  - empty toolbar chrome
  - child controls (e.g. ComboBox)

Does **not**:

- Own semantic identity
- Hard-code toolbar or menu bar IDs

### MenuHostControl

Owns:

- Projection of a menu bar
- Mapping registry visibility → `Visibility`

Does **not**:

- Toggle visibility directly
- Contain hard-coded semantic identity

---

## Semantic Identity Ownership

A critical refinement from the migration:

> The framework must **never** dictate semantic identity.

As a result:

- `ToolbarId` is the sole semantic identifier
- Framework controls must accept identity via configuration
- Defaults are permissible for convenience, but never enforced

This enables:

- Multiple menu bars in a single application
- Independent control over multiple hosts
- BBApp.Next compatibility

---

## MenuBarId Configuration

To preserve host ownership of semantics, projection controls expose:

- `MenuBarId : ToolbarId` dependency property

On:

- `ToolbarHostControl`
- `MenuHostControl`

Behavioral contract:

- If `MenuBarId` is provided, it is authoritative
- If not provided, a conventional default may be used
- No projection logic may rely on hard-coded string identifiers

---

## Toolbar Item Rebuild Rules

The following rules are now formalized:

- Toolbar items may be rebuilt freely by the application
- Projection must tolerate:
  - ItemsSource replacement
  - In-place collection mutation
- Registry visibility state must **always** be reapplied
- Presentation correctness must not depend on item identity stability

This invariant enables complex runtime scenarios without semantic drift.

---

## Context Menu Ownership

Another clarification:

- Toolbar customization context menus are owned by the **toolbar host**, not individual items
- Host-level preview handling is required to ensure:
  - Empty chrome participates
  - Child controls do not preempt customization

Menu composition must reflect semantic grouping without reintroducing legacy special cases.

---

## Explicit Anti-Patterns (Do Not Reintroduce)

The migration demonstrated several patterns that must remain prohibited:

- Hard-coded semantic IDs (e.g. "MenuBar")
- Item-owned or ViewModel-owned visibility
- Special-case menu bar logic in projection code
- Visibility bound directly in XAML
- Treating `ToolbarItem` as a presentation ViewModel

These patterns undermine persistence, portability, and correctness.

---

## Architectural Outcome

With the completion of this migration:

- ADR-0002 is fully realized in code
- Toolbars and menu bars share a single semantic visibility model
- Projection behavior is stable, predictable, and extensible
- The framework is prepared for BBApp.Next projection layers

This addendum captures the *operational truth* of the design and should be consulted alongside ADR-0002 for all future work.
