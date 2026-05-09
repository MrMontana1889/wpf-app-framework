# ADR-0003: Generic Toolbar Visibility Management System

## ADR Number
ADR-0003

## Title
Introduce a Generic Toolbar Visibility and Customization System in Dev.Core and Dev.Wpf

## Status
Accepted — March 26, 2026

## Context
Prior to this decision, each view in BentleyBuildApp.Next that required a toolbar owned its own bespoke `UserControl` (e.g., `BuildToolbar`, `ProjectToolbar`). These per-view controls duplicated visibility logic, did not share state, could not be toggled by the user, and had no persistence. There was no consistent user-facing mechanism to show or hide toolbars, no right-click context menu, and no customization dialog.

BentleyBuildApp.Next is designed as a long-lived platform (see ADR-0001). As the application grows, new feature areas will introduce additional toolbars. Without a shared infrastructure:

- Each new toolbar would re-implement its own visibility toggle, persistence, and right-click menu logic.
- Theme-aware context menu rendering would have to be duplicated or would diverge between toolbars.
- Testing would be isolated per feature, with no shared contract to validate against.

The architecture establishes `Dev.Core` as the home for platform-agnostic services and view models and `Dev.Wpf` as the home for shared WPF controls and themes. A generic, reusable toolbar visibility system belongs in these layers — not in application-level code — so that it can be consumed by `BentleyBuild` / `BentleyBuildApp` today and by future products tomorrow.

This decision was implemented as part of PR #10, which delivered 184 passing NUnit tests covering all new components.

## Decision
A complete toolbar visibility and customization infrastructure is introduced, split across `Dev.Core` (platform-agnostic models and services) and `Dev.Wpf` (WPF control, converters, styles, and dialog), and consumed by `BentleyBuild` (domain view models) and `BentleyBuildApp` (registration and wiring).

### Dev.Core Responsibilities

**Registry and persistence services:**
- `IToolbarRegistryService` / `ToolbarRegistryService` — centralized registration of toolbar instances at application startup. Each toolbar is registered with a `canHide` policy: toolbars registered with `canHide: false` appear in the context menu as read-only entries that cannot be toggled off.
- `IToolbarSettingsService` / `ToolbarSettingsService` — persists toolbar visibility state to disk and restores it automatically on next launch.
- `ToolbarItemSettings` — serializable model carrying per-toolbar persistence state.

**Toolbar view models:**
- `ToolbarModel` — base view model extended with `IsToolbarVisible`, `CanHide`, `ToggleVisibilityCommand`, and `CustomizeCommand`. `ToggleVisibilityCommand` enforces the `CanHide` policy: non-hideable toolbars are immune to toggle regardless of invocation source.
- `ToolbarCustomizeMenuEntry` / `ToolbarItemCustomizeEntry` / `ToolbarItemModel` — view model types composing the item list presented in the customize dialog.
- `CustomizeToolbarViewModel` — full MVVM dialog view model. Presents the toolbar's item list as a checklist; `Apply` commits visibility changes and persists them, `Cancel` discards without saving.
- `AboutDialogViewModel` — companion dialog view model (pre-existing, colocated in the same layer).

**Service abstractions:**
- `IDialogService`, `IMessageBoxService`, `IClipboardService` — dialog and interaction abstractions kept in `Dev.Core` so view models can open dialogs without taking a WPF dependency.

### Dev.Wpf Responsibilities

**Reusable control:**
- `ToolbarControl` — single generic `UserControl` driven entirely by a `ToolbarModel`. Replaces all per-view toolbar `UserControl`s. Right-clicking anywhere on the toolbar opens a context menu listing all registered toolbars. Each entry renders as a checkable `MenuItem` reflecting current `IsToolbarVisible` state.

**Converters:**
- `ToolbarContextMenuConverter` — multi-value converter that assembles the flat context menu item list from all registered toolbars plus the optional "Customize…" separator entry.

**Style selector:**
- `ToolbarMenuItemStyleSelector` — dispatches the correct keyed `MenuItem` style per item type (toggle vs. customize), with both styles inheriting the theme's implicit `MenuItem` template via `BasedOn`.

**Theme-aware styling:**
- Dark and light themes each provide a fully custom `ContextMenu` and `MenuItem` `ControlTemplate`. The check column uses a `Path`-based checkmark glyph rather than the system default, ensuring correct rendering in dark mode. Disabled checked items dim the checkmark stroke independently of item foreground, covering the checked-and-disabled state.

**Dialog:**
- `CustomizeToolbarDialog` — XAML dialog backed by `CustomizeToolbarViewModel`, wired through `IDialogService` and constructed via dependency injection.

### BentleyBuild / BentleyBuildApp Responsibilities

- `BuildToolbarModel` and `ProjectToolbarModel` extend `ToolbarModel` with domain-specific toolbar button items.
- `BentleyBuildViewModel` exposes `IsToolbarSeparatorVisible`, which is `true` only when both the project and build toolbars are visible. Property-change chaining via `CommunityToolkit.Mvvm` on both toolbar models drives the separator automatically.
- At application startup (`BentleyBuildApp`), `ProjectToolbarModel` is registered with `canHide: false` (always visible) and `BuildToolbarModel` with the default `canHide: true` policy.
- `BentleyBuildDialogService` / `IBentleyBuildDialogService` provide the application-level dialog host wired to `Dev.Wpf` dialogs.

## Rationale

### Motivation for a generic, cross-application system
A toolbar visibility toggle, context menu, persistence, and customization dialog are not features specific to any one toolbar or any one product. Implementing them generically in `Dev.Core` and `Dev.Wpf` means every future toolbar in BentleyBuildApp.Next and every future product that consumes these libraries gains the full capability at zero additional implementation cost.

### Toolbar and button visibility managed through Dev.Core
All visibility state, toggle commands, persistence reads/writes, and the `canHide` policy live in `Dev.Core` view models and services. This means the full behavioral contract — including edge cases such as toggling a non-hideable toolbar — is testable with NUnit without any WPF runtime. The 184 tests in `Dev.Core.Tests` and `BentleyBuild.Tests` validate this contract exhaustively.

### Dev.Wpf participates in UI binding and MVVM compliance
`Dev.Wpf` owns only the presentation layer: the `ToolbarControl` XAML, converters, style selectors, and theme resources. No business logic lives in `Dev.Wpf` code-behind. All state flows through bindings to `ToolbarModel` properties. This maintains strict MVVM compliance as required by the project architecture.

### Why this subsystem belongs in Dev.Core / Dev.Wpf, not application-level code
Placing the toolbar system in application-level code (`BentleyBuild` or `BentleyBuildApp`) would couple it to a single product's lifecycle, prevent reuse, and force duplication if a second product is introduced. `Dev.Core` and `Dev.Wpf` are explicitly designed as reusable platform layers (ADR-0001). Toolbar infrastructure is a platform-level concern, not a domain-specific one.

### Reusability, consistency, and future extensibility
Any new toolbar in any future feature area needs only to:
1. Create a `ToolbarModel` subclass in the appropriate non-UI project.
2. Register it with `IToolbarRegistryService` at startup with the desired `canHide` policy.
3. Bind a `ToolbarControl` in XAML to the model.

Theme-aware styling, right-click menus, persistence, and the customize dialog are inherited automatically. No duplication, no divergence.

## Alternatives Considered

### Per-view toolbar UserControls with bespoke visibility logic
Continue the prior approach: each toolbar owns its own `UserControl` with its own show/hide toggle.  
**Rejected** because: logic diverges across toolbars over time, there is no shared persistence contract, testing is fragmented, and adding the right-click context menu to every toolbar independently is a significant duplication burden.

### Application-level toolbar registry (inside BentleyBuild or BentleyBuildApp)
Place `IToolbarRegistryService` and related view models inside `BentleyBuild` or `BentleyBuildApp` rather than `Dev.Core`.  
**Rejected** because: it ties the toolbar infrastructure to a single product, prevents reuse in future products, and violates the principle that `Dev.Core` is the home for shared platform-level services.

### Third-party toolbar / ribbon framework
Use an existing WPF toolbar/ribbon library (e.g., Fluent.Ribbon) rather than implementing a custom system.  
**Rejected** because: the toolbar requirements are intentionally minimal (show/hide, persist, context menu, customize dialog); a full ribbon framework would introduce a large external dependency with significant theming complexity and would not integrate cleanly with the existing PropertyTools-based property grid infrastructure.

### WPF CommandBar / ToolBar + ContextMenu without a registry service
Rely on WPF's built-in `ToolBar` and `ContextMenu` primitives directly in XAML, with visibility toggled via `Visibility` bindings, without a centralized registry.  
**Rejected** because: without a registry, dynamic context menu generation (listing all toolbars by name) requires either hard-coded XAML or fragile reflection-based discovery. Persistence and the `canHide` policy would have no natural home. The registry pattern is the only approach that scales cleanly to an arbitrary number of toolbars.

## Consequences

### Positive
- Any future toolbar requires only a model subclass and a registration call; all UX behavior is inherited automatically.
- Full behavioral coverage (184 NUnit tests) validates the visibility, persistence, `canHide` policy, and customize dialog contract without a WPF runtime.
- Theme-aware context menus and checkmark glyphs ensure pixel-correct rendering in both dark and light modes with no per-toolbar theme work.
- `IsToolbarSeparatorVisible` on `BentleyBuildViewModel` demonstrates property-change chaining: the separator tracks toolbar visibility automatically, keeping the layout clean without imperative code.
- Strict MVVM compliance is maintained: no business logic in `Dev.Wpf` code-behind; all state flows through bindings.
- The `canHide: false` policy ensures safety-critical or mandatory toolbars cannot be accidentally hidden by users or by automated commands.

### Negative
- **Centralization vs. flexibility trade-off**: all toolbars share the same `ToolbarModel` contract. Highly bespoke toolbar behaviors (e.g., a toolbar that is only visible in a specific mode, not driven by a simple boolean) require extending `ToolbarModel` rather than building a completely custom solution, which may constrain unusual future requirements.
- **Registry coupling at startup**: application code must register all toolbars at startup via `IToolbarRegistryService`. Late or dynamic registration (toolbars appearing/disappearing based on runtime feature flags) is possible but requires additional `IToolbarRegistryService` API surface that is not yet implemented.
- **Persistence format coupling**: `ToolbarSettingsService` serializes to a fixed schema. Changes to toolbar IDs or names require a migration strategy to avoid losing user preferences across application updates.
- **Dev.Wpf test gap**: `ToolbarControl` XAML rendering, context menu layout, and style selector behavior are not covered by automated tests (see ADR-0001 follow-up: no `Dev.Wpf.Tests` project). Theme-aware rendering must be validated manually when themes are updated.

### Follow-Up Work
- Define a toolbar ID stability policy: toolbar registration names must not change across releases without a persistence migration path.
- Extend `IToolbarRegistryService` with dynamic registration/unregistration support when runtime feature-flag-driven toolbars are required.
- Add a `Dev.Wpf.Tests` project (tracked in ADR-0001) and include at minimum a headless `ToolbarContextMenuConverter` test to validate context menu item assembly logic without requiring a full WPF host.
- Consider introducing a `ToolbarGroup` concept in the registry to support logical grouping of toolbars within the context menu as the number of registered toolbars grows.
- Document the toolbar registration pattern in the repository wiki as the canonical example for onboarding new feature areas.

## References
- [ADR-0001-Project-Structure.md](ADR-0001-Project-Structure.md) — Defines the project boundaries this subsystem respects
- [ADR-Template-BBAppNext.md](ADR-Template-BBAppNext.md) — Template used for this ADR
- `src/Dev.Core/Services/Toolbar/IToolbarRegistryService.cs`
- `src/Dev.Core/Services/Toolbar/ToolbarRegistryService.cs`
- `src/Dev.Core/Services/Toolbar/IToolbarSettingsService.cs`
- `src/Dev.Core/Services/Toolbar/ToolbarSettingsService.cs`
- `src/Dev.Core/Services/Toolbar/ToolbarItemSettings.cs`
- `src/Dev.Core/ViewModels/CustomizeToolbarViewModel.cs`
- `src/Dev.Core/ViewModels/Controls/ToolbarModel.cs`
- `src/Dev.Wpf/Controls/ToolbarControl.xaml`
- `src/Dev.Wpf/Converters/ToolbarContextMenuConverter.cs`
- `src/Dev.Wpf/Controls/ToolbarMenuItemStyleSelector.cs`
- `src/Dev.Wpf/Views/CustomizeToolbarDialog.xaml`
- `src/Dev.Wpf/Themes/DarkTheme.xaml` / `src/Dev.Wpf/Themes/LightTheme.xaml`
- `src/BentleyBuild/ViewModels/Controls/BuildToolbarModel.cs`
- `src/BentleyBuild/ViewModels/Controls/ProjectToolbarModel.cs`
- `src/BentleyBuild/ViewModels/BentleyBuildViewModel.cs`
- `tests/Dev.Core.Tests/Services/ToolbarRegistryServiceTests.cs`
- `tests/Dev.Core.Tests/Services/ToolbarSettingsServiceTests.cs`
- `tests/Dev.Core.Tests/ViewModels/CustomizeToolbarViewModelTests.cs`
- `tests/Dev.Core.Tests/ViewModels/Controls/ToolbarModelTests.cs`
- `tests/BentleyBuild.Tests/ViewModels/Controls/BuildToolbarModelTests.cs`
- `tests/BentleyBuild.Tests/ViewModels/Controls/ProjectToolbarModelTests.cs`
- PR #10 — "Add toolbar visibility management with context menu, customize dialog, and theme-aware styling"
