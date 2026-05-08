# ADR-0004: TreeViewControl Subsystem Architecture

## ADR Number
ADR-0004

## Title
TreeViewControl Subsystem Architecture for BentleyBuildApp.Next

## Status
Accepted — March 27, 2026

## Context
BentleyBuildApp.Next requires a hierarchical tree view control capable of handling large node sets
with search and filtering, multi-select, lazy loading, tri-state checkboxes, drag-drop reordering,
per-node context menus, and theme-aware icons. No existing WPF component satisfies all of these
requirements without introducing legacy dependencies or violating the project's strict MVVM and
project-boundary rules.

This decision operates within the two-layer architecture established by ADR-0001:

- **Dev.Core** (`net10.0`) — platform-agnostic; must not reference any WPF assembly.
- **Dev.Wpf** (`net10.0-windows`) — WPF-specific; owns all controls, templates, behaviors,
  and converters.

The subsystem is designed for long-term reuse beyond BentleyBuildApp.Next and is a candidate
for future open-source extraction. All non-UI logic must be independently unit-testable
with NUnit, without a WPF runtime.

## Decision

A custom `TreeViewControl` subsystem is introduced, split across `Dev.Core` and `Dev.Wpf`
according to the responsibilities below.

---

### Dev.Core — Logic Layer

#### Node Model (`TreeNodeModel`)

Each node in the tree is represented by a single model class with the following members:

| Member | Description |
|---|---|
| `string Id` | Stable identifier used for diffing and persistence |
| `string Label` | Primary display text |
| `string? IconKey` | Opaque string token resolved to `ImageSource` only in Dev.Wpf |
| `bool IsExpandable` | Whether the node can be expanded |
| `bool IsExpanded` | Tracks current expansion state |
| `bool IsSelected` | Whether the node is part of the current selection |
| `bool? IsChecked` | Optional tri-state checkbox value (`true`, `false`, `null`) |
| `IReadOnlyList<TreeNodeModel> Children` | Immediate child nodes |
| `Func<Task<IReadOnlyList<TreeNodeModel>>>?` `LazyLoadCallback` | Null for eager nodes |
| `bool IsLazyLoaded` | Set after callback resolves |
| `bool IsMatch` | Set by search engine when node satisfies the query |
| `bool IsAncestorOfMatch` | Set by search engine when a descendant satisfies the query |
| `int SelectionIndex` | Internal ordering used for range-selection |
| `bool IsSelectable` | Per-node opt-out of selection |
| `bool SupportsContextMenu` | Per-node opt-out of context menus |

#### Lazy Loading

Two modes are supported:

- **Eager** — children are populated at construction time; `LazyLoadCallback` is null.
- **Lazy** — `LazyLoadCallback` is invoked on first expansion; once resolved, children
  are populated and `IsLazyLoaded` is set to `true`.

Dev.Core owns the callback contract and state transitions. Dev.Wpf observes `IsExpanded`
and triggers the callback — it carries no loading logic of its own.

#### Search Engine

The search engine accepts a filter and produces a new, pruned tree containing only matching
nodes and their ancestors. The original tree is never mutated. Three search surfaces are
supported, each building on the one below:

1. **Predicate search** — accepts `Func<TreeNodeModel, bool>` directly; the primitive on
   which all other modes are built.
2. **Property search** — `Search(propertyName, value)` compiles a named property to a cached
   accessor delegate and builds a predicate.
3. **Query Object DSL (`TreeQuery`)** — an immutable, composable query object that compiles
   to a predicate at evaluation time.

`TreeQuery` supports:

- Text matching: `Contains`, `Equals` (with optional case sensitivity)
- Numeric comparisons: `GreaterThan`, `LessThan`, `Between`
- Boolean checks
- Logical combinators: `And`, `Or`, `Not`
- User-supplied predicates as escape hatch

Example:

```csharp
var query = TreeQuery.For("Label").Contains("Pipe")
                     .And(TreeQuery.For("IsChecked").EqualTo(true));
```

The filtered result sets `IsMatch` and `IsAncestorOfMatch` on every node, enabling Dev.Wpf
to highlight matches and auto-expand matched branches without the UI performing any tree
traversal itself.

#### Checkbox Semantics

Tri-state propagation rules — parent state computed from children, child state inherited
from parent — are implemented entirely in Dev.Core so they can be unit-tested in isolation.

#### Drag-Drop Rule Evaluation

Whether a set of dragged nodes may be dropped onto a target node is evaluated in Dev.Core.
Dev.Wpf calls into this rule evaluation during hover and drop events; it owns only the
visual feedback.

#### Multi-Select Model

Selection rules — range expansion (Shift+Click), toggle (Ctrl+Click), right-click
non-destructive selection — are validated in Dev.Core through a dedicated selection rule
engine. `SelectionMode` (`None | Single | Multiple`) is a property of the control but
the rule evaluation is core-layer logic.

#### Icon Key Registry

To avoid magic strings, a static `IconKeys` class in Dev.Core provides named constants:

```csharp
public static class IconKeys
{
    public const string Folder         = "Folder";
    public const string CsFile         = "CsFile";
    public const string BuildStrategy  = "BuildStrategy";
    public const string Warning        = "Warning";
    public const string Error          = "Error";
}
```

Domain code assigns `IconKey` from this registry or any custom string. No WPF type is
referenced.

#### Context Menu Model

The data model for context menu items lives in Dev.Core:

```csharp
public sealed record MenuItemModel(
    string Label,
    ICommand Command,
    object? CommandParameter = null,
    string? IconKey = null,
    bool IsEnabled = true,
    IReadOnlyList<MenuItemModel>? Children = null);
```

Menu construction is delegated to an interface also defined in Dev.Core (or on the boundary):

```csharp
public interface ITreeContextMenuProvider
{
    IReadOnlyList<MenuItemModel> BuildMenu(
        IReadOnlyList<TreeNodeModel> selectedNodes);
}
```

This ensures menus are built against the abstract node model, not against WPF types.

---

### Dev.Wpf — Presentation Layer

#### TreeViewControl

The control derives from `ItemsControl` (not the built-in WPF `TreeView`) and uses a
recursive `ItemsControl` pattern for child levels. This architecture enables:

- `VirtualizingStackPanel` in recycling mode for high-performance rendering of large trees.
- Full template control over every aspect of node appearance.
- Avoidance of `TreeViewItem`'s hardcoded visual states, focus management, and eager
  visual-tree realization.

The public API surface:

```csharp
// Expansion
void ExpandAll();
void CollapseAll();

// Search
void Search(TreeQuery query);
void ClearSearch();

// Lazy
void RefreshLazyLoadedNodes();

// Selection
SelectionMode SelectionMode { get; set; }
IReadOnlyList<TreeNodeModel> SelectedNodes { get; }
```

#### Event Surface

The control raises a comprehensive set of typed events so consumers can react without
subclassing:

```csharp
// Selection
event EventHandler<SelectionChangedEventArgs>      SelectionChanged;
event EventHandler<MultiSelectionChangedEventArgs> MultiSelectionChanged;

// Activation
event EventHandler<NodeActivatedEventArgs>         NodeActivated;    // double-click or Enter

// Structure
event EventHandler<NodeExpandedEventArgs>          NodeExpanded;
event EventHandler<NodeCollapsedEventArgs>         NodeCollapsed;

// Context menu
event EventHandler<ContextMenuOpeningEventArgs>    ContextMenuOpening;

// Drag & drop
event EventHandler<NodesDroppedEventArgs>          NodesDropped;

// Checkbox
event EventHandler<NodeCheckedChangedEventArgs>    NodeCheckedChanged;
```

#### Multi-Select Behavior

A `MultiSelectBehavior` attached behavior implements:

- **Ctrl+Click** — toggle individual node selection.
- **Shift+Click** — select a contiguous range using `SelectionIndex` ordering.
- **Right-click** — selects the target node when it is not already part of the active
  selection (preserves multi-selection otherwise).
- **Keyboard navigation** — Up/Down, Shift+Up/Down range extension, optional Ctrl+A.

Selection rule validation is delegated to the Dev.Core selection rule engine; the behavior
owns only the WPF input handling and visual feedback.

#### Context Menu System

A `ContextMenuBehavior` constructs the context menu dynamically at right-click time:

1. Raises `ContextMenuOpening` (allowing cancellation or augmentation by the host).
2. Calls `ITreeContextMenuProvider.BuildMenu(selectedNodes)` with the current selection.
3. Converts the returned `IReadOnlyList<MenuItemModel>` to WPF `MenuItem` elements, using
   `IconConverter` to resolve any `IconKey` values.

`SupportsContextMenu` on `TreeNodeModel` is respected: if all selected nodes opt out, the
menu is suppressed. MVVM compliance is maintained — no business logic runs in code-behind.

#### IIconProvider and Icon Resolution

Dev.Wpf defines the icon resolution interface:

```csharp
public interface IIconProvider
{
    ImageSource GetIcon(string iconKey);
}
```

An `IconConverter` (`IValueConverter`) calls the active `IIconProvider` instance at binding
time:

```xml
<Image Source="{Binding IconKey, Converter={StaticResource IconConverter}}" />
```

Named provider implementations:

| Provider | Description |
|---|---|
| `DefaultIconProvider` | Resolves pack URIs to embedded PNG resources |
| `VectorIconProvider` | Loads XAML-defined vector geometry |
| `ThemedIconProvider` | Wraps two providers; delegates based on active theme |
| `ChainedIconProvider` | Fallback chain across multiple providers |

Providers cache resolved `ImageSource` instances and call `Freeze()` to minimize memory
pressure and enable cross-thread access.

#### Interaction Behaviors

All interaction logic is implemented as attached behaviors:

- `ExpandCollapseBehavior` — click-to-toggle, keyboard expand/collapse
- `CheckboxBehavior` — toggle with tri-state propagation via Dev.Core rules
- `DragDropBehavior` — drag start, hover hit-test, insert-marker visuals, drop event
- `KeyboardNavigationBehavior` — arrow keys, Enter activation, Ctrl+A
- `MultiSelectBehavior` — see above
- `ContextMenuBehavior` — see above

No interaction logic appears in XAML code-behind files.

#### Search Highlighting and Templates

When a search is active, the control auto-expands ancestor nodes of matched results
(configurable) and applies inline highlight styling to `Label` text driven by the
`IsMatch` flag. All visual decoration is data-driven from node model properties,
with no imperative code-behind.

#### Theming

Icon providers and control templates participate in BBApp.Next's existing theme system
(defined in `Dev.Wpf/Themes`). `ThemedIconProvider` switches icon sets when the active
theme changes.

---

## Rationale

### Why a Custom Control Instead of the Built-In WPF TreeView?

WPF's `TreeView` / `TreeViewItem` has well-documented performance degradation at scale:
it eagerly realizes the full visual tree, its virtualization support is incomplete, and
its hardcoded visual states, selection handling, and focus management cannot be overridden
without invasive subclassing. Building from `ItemsControl` with `VirtualizingStackPanel`
in recycling mode avoids all of these constraints and gives complete template control. The
multi-select, context menu, and event surface requirements further reinforce this choice —
implementing them on top of  `TreeViewItem` would require fighting the control's built-in
behaviors at every step.

### Why Split Logic Across Dev.Core and Dev.Wpf?

ADR-0001 mandates that `Dev.Core` targets `net10.0` with no WPF references. Keeping node
models, search, lazy load contracts, checkbox rules, selection rules, and context menu
models in Dev.Core makes the full logic layer independently unit-testable with NUnit,
without any WPF runtime. Dev.Wpf adds only the rendering surface and input handling
on top of the tested core.

### Why `string IconKey` Rather Than `ImageSource` on the Model?

Storing a WPF `ImageSource` on `TreeNodeModel` would force Dev.Core to reference
`PresentationCore`, violating the `net10.0` boundary in ADR-0001. The opaque `string`
token costs nothing in the core layer and is resolved to `ImageSource` only at binding
time inside Dev.Wpf. This also enables OSS portability — the same model works in a
console or server context with no UI subsystem.

### Why `ITreeContextMenuProvider` Rather Than Direct XAML Menus?

A XAML-only context menu would require duplicating menu logic per node type and cannot
be multi-selection aware without code-behind. An injectable provider interface allows
menu construction to be data-driven, testable, and responsive to the full current
selection. It also enables downstream applications to substitute their own provider
without modifying the control.

### Why the TreeQuery DSL in Addition to Predicates?

A bare `Func<TreeNodeModel, bool>` is sufficient internally but is not serializable,
inspectable, or composable from external call sites. The `TreeQuery` DSL represents
search intent as an immutable, combinable value that can be stored, logged, and
re-evaluated. It compiles to a predicate at evaluation time, so there is no additional
runtime cost relative to a hand-written predicate. Both surfaces are preserved: the DSL
is additive, not a replacement.

### Why a Formal Event Surface?

Controls that communicate results only through binding updates force consumers to add
boilerplate watchers on observable collections. Typed events (`NodeActivated`,
`NodesDropped`, `ContextMenuOpening`, etc.) provide a conventional, discoverable
integration surface that is compatible with command-handler patterns and does not require
consumers to reference internal model internals.

---

## Alternatives Considered

### Use the Built-In WPF TreeView
Rejected. The native `TreeView` / `TreeViewItem` lacks recycling virtualization, imposes
inflexible selection and focus behavior, and degrades under large node counts. The
multi-select and context-menu requirements make direct use unviable without more invasive
customization than building from `ItemsControl`.

### Integrate a Third-Party Tree Control
Rejected. Commercial or third-party tree controls introduce external licensing dependencies,
impede future OSS extraction, and reduce architectural transparency. The requirements are
well-scoped and achievable within the existing project structure.

### Store `ImageSource` Directly on `TreeNodeModel`
Rejected. Would require Dev.Core to reference `PresentationCore`, violating the `net10.0`
no-WPF-dependency constraint in ADR-0001 and breaking portability.

### XAML-Only Context Menus (No Provider Interface)
Rejected. A static XAML menu cannot be multi-selection-aware, cannot be injected or
replaced by consumers, and embeds business logic in templates. The `ITreeContextMenuProvider`
interface is necessary for correctness and testability.

### Predicate-Only Search (No TreeQuery DSL)
Considered and partially retained. Predicate search is the foundation layer and is always
available. `TreeQuery` is additive — it compiles to a predicate. Omitting the DSL would
reduce API expressiveness and composability without any architectural benefit.

### Built-In WPF Multi-Select ListBox/ListView as Host
Rejected. Multi-level hierarchy with lazy loading, search filtering, and per-node rules
is not expressible in a flat list paradigm. A recursive `ItemsControl` is the only approach
that cleanly handles arbitrary tree depth.

---

## Consequences

### Positive
- All node model, search, lazy load, checkbox, selection, and context-menu model logic
  is unit-testable in `Dev.Core.Tests` without a WPF runtime.
- Strict project boundary compliance with ADR-0001 is maintained throughout.
- Recycling virtualization delivers performant rendering for large node trees.
- `IIconProvider` / `ITreeContextMenuProvider` interfaces support DI injection, theming,
  and per-application customization without modifying the control.
- The typed event surface provides a clean, discoverable integration contract for consumers.
- `TreeQuery` DSL provides composable, serializable search suitable for OSS consumers.
- The subsystem is reusable across future WPF products with no BBApp-specific coupling.

### Negative
- Building a custom tree control requires significantly more initial implementation effort
  than using the built-in `TreeView`.
- The `IIconProvider` and `ITreeContextMenuProvider` abstractions add indirection that must
  be registered via DI and wired correctly at application startup.
- The `TreeQuery` DSL introduces a small grammar surface that contributors must understand.
- The full behavior suite (multi-select, context menu, drag-drop, keyboard navigation)
  requires more behavior classes than a simpler, single-behavior approach.

### Follow-Up Work
- **Phase A** — Implement `TreeNodeModel`, lazy load plumbing, and predicate search engine
  in Dev.Core.
- **Phase B** — Implement `TreeQuery` object model, query compiler, and integration with
  the search engine.
- **Phase C** — Implement `ItemsControl`-based `TreeViewControl` with virtualization and
  expand/collapse template in Dev.Wpf.
- **Phase D** — Add checkbox tri-state, drag-drop, search highlighting, keyboard
  navigation, context menu architecture, multi-select behavior, and selection/activation
  events.
- **Phase E** — Finalize public API and event surface, write unit tests, integrate into
  `BentleyBuildApp`, and produce developer documentation.
- **Phase F** *(future)* — Extract Dev.Core tree components and Dev.Wpf tree control into
  a standalone OSS library with default icon pack and full API documentation.

---

## References
- [ADR-0001 — BentleyBuildApp.Next Project Structure](ADR-0001-Project-Structure.md)
- [TreeViewControl High-Level Design Document](../design/TreeViewDesign.md)
- [TreeViewControl Icon System Design Document](../design/TreeViewIconDesign.md)
