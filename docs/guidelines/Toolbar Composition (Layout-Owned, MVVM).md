# Toolbar Composition – Layout‑Based MVVM Guideline

> Status: Guideline / Intentional Direction (non‑binding)
> 
> Purpose: Document a secondary toolbar approach for WPF applications that complements (but does not replace) the existing registry‑based toolbar visibility system.

* * *

## Why This Exists

The existing toolbar registry + visibility system is correct, stable, and actively used (e.g., BBA.Next). However, some applications—Personal Ledger (PL) in particular—benefit from a layout‑owned, MVVM‑driven toolbar composition model that aligns more closely with native WPF patterns and app‑shell design.

This guideline documents that alternative without committing to concrete implementation details.

Key goals:

* Preserve the existing system unchanged
* Allow application shells to define what appears in a toolbar, not merely whether a toolbar exists
* Stay WPF‑idiomatic and MVVM‑pure
* Be explicit that this is a wrapper around WPF ToolBar, not a replacement

* * *

## Non‑Goals

This guideline does not attempt to:

* Replace the toolbar registry
* Solve cross‑application toolbar customization
* Define persistence, drag‑drop, or user configuration
* Require feature parity with the registry system

Those concerns remain intentionally out of scope.

* * *

## Conceptual Model

### Core Idea

> The application shell owns the toolbar layout.
> 
> Toolbars are composed from ViewModel‑driven items, projected directly into WPF `ToolBar` controls using standard data binding.

The framework provides a thin ViewModel abstraction that describes toolbar items, not toolbars as global entities.

* * *

## Supported Item Types (Initial Scope)

The composition layer should support, at minimum:

* Button
* ToggleButton
* CheckBox
* Label
* ComboBox

> NOTE: RadioButtons are intentionally excluded for now; their inclusion can be revisited if a real use case emerges.

* * *

## Display Capabilities

### Buttons & ToggleButtons

A button‑style toolbar item should support:

* Icon‑only
* Text‑only
* Icon + text

Display choice is owned by the ViewModel, not inferred by the View.

* * *

### CheckBox

A checkbox toolbar item should support:

* Label only
* (Optionally) label combined with an icon

Icon usage is explicitly left open for future clarification.

* * *

### ComboBox

A combobox toolbar item should support:

* Label only
* Label + ComboBox

As with CheckBox, icon support is intentionally undefined at this stage.

* * *

## MVVM Expectations

This system is explicitly MVVM‑first:

* Toolbar items are represented by ViewModels, not Views
* Commands, state, and visibility live in ViewModels
* No code‑behind is expected at the shell level
* Styling and visuals are provided entirely by XAML and theming

The View layer is responsible only for:

* Choosing the appropriate WPF control type
* Applying theme‑aware styles
* Binding to exposed ViewModel properties

* * *

## Relationship to Native WPF

This is intentionally a wrapper, not a reinvention.

Under the hood, the system should rely on:

* `ToolBar`
* `ToolBarTray`
* `ItemsControl`
* `DataTemplate` / `DataTemplateSelector`

The abstraction exists to:

* Provide a consistent ViewModel shape
* Avoid leaking WPF control logic into application ViewModels
* Enable future growth without committing early

* * *

## Visibility & Enablement

Visibility and enablement are:

* Item‑level concerns
* Derived from ViewModel state
* Potentially influenced by application mode or context

There is no concept of registering or hiding entire toolbars in this approach.

* * *

## Theming Considerations

All visuals:

* Must be fully theme‑derived
* Must not hardcode colors, brushes, or fonts
* Must respect existing menu / toolbar / status bar theme infrastructure

This guideline explicitly assumes the application framework is theme‑ready.

* * *

## Coexistence With the Registry System

The two toolbar approaches are designed to coexist:

| Registry‑Based Toolbars | Layout‑Based Toolbars |
| --- | --- |
| Feature‑centric | Shell‑centric |
| Toolbar‑level visibility | Item‑level composition |
| Global registration | Local ownership |
| Heavy customization | Predictable layout |

An application may use one or both, depending on context.

* * *

## Guiding Principle

> Choose the simplest tool that preserves intent.

For application shells and mode‑driven UI, layout‑based composition is often clearer.

For feature‑rich, customizable environments, the registry system remains the right solution.

* * *

## Future Considerations (Explicitly Deferred)

* User customization
* Persistence
* Drag‑and‑drop
* Shared toolbar definitions
* Registry interoperability

These concerns should be addressed only when real product pressure exists.

* * *

## Summary

Yes—this approach makes sense.

Documenting it now:

* Establishes intent
* Prevents accidental misuse of the registry system
* Gives PL a clean architectural lane
* Avoids premature design commitments

This guideline should be treated as architectural guardrails, not a contract.