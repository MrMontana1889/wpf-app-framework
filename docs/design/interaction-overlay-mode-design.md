# Interaction Overlay Mode – Full Design Specification

## Overview

The Interaction Overlay Mode introduces a composable, non-modal interaction layer that sits above an active FeatureMode. It replaces traditional modal dialogs with a unified, testable, framework-agnostic pattern for collecting user input.

An InteractionOverlay is defined as:

> A transient interaction session that collects input and produces a result without directly executing domain logic.

This pattern generalizes simple prompts, editors, and multi-step wizards into a single abstraction.

---

## Goals

- Eliminate modal dialog windows
- Preserve context of underlying FeatureMode
- Enable simple and complex interaction flows
- Maintain full testability (headless)
- Keep domain operations centralized in FeatureMode
- Provide consistent UX behavior across all interaction flows

---

## Core Concepts

### FeatureMode (existing)
- Owns primary UI surface
- Executes domain operations
- Observes notifications and refreshes projections

### InteractionOverlay (new)
- Collects input
- Produces a result (TResult)
- Does NOT execute domain logic

### OverlayHost (UI layer)
- Renders overlays
- Manages layout, stacking, and interaction blocking

---

## Interface Design

```csharp
public interface IInteractionOverlay
{
    void OnEnter();
    void OnExit();
}

public interface IInteractionOverlay<TResult> : IInteractionOverlay
{
    void SetResultCallback(Action<TResult> callback);
}
```

---

## ModeService Extensions

```csharp
public interface IModeService
{
    IFeatureMode? CurrentMode { get; }

    void EnterMode(IFeatureMode mode);
    void ExitMode();

    IReadOnlyList<IInteractionOverlay> ActiveOverlays { get; }

    void ShowOverlay<TResult>(
        IInteractionOverlay<TResult> overlay,
        Action<TResult> onResult);

    void CloseOverlay(IInteractionOverlay overlay);
}
```

---

## Behavioral Rules

### Overlay Behavior

- Overlays are stacked (LIFO)
- Only top overlay is interactive
- Underlying UI is blocked
- ESC always triggers cancel
- Overlay invokes callback when complete

### FeatureMode Responsibilities

- Initiates overlay
- Receives result
- Executes domain operation

### Domain Interaction Rule

Overlays MUST NOT execute domain operations directly.

---

## Supported Use Cases

| Type | Example | Result |
|------|--------|--------|
| Simple prompt | Confirm delete | bool |
| Editor | Rename category | string |
| Wizard | New account flow | NewAccountData |

---

## Overlay Stacking

- Stack-based (push/pop)
- Only top overlay receives input
- Lower overlays are inactive

---

## UI Architecture

### Layering

```
Application Shell
 ├── Toolbar
 ├── FeatureMode Content
 └── OverlayHost (top layer)
```

### Overlay Host Responsibilities

- Full-window coverage
- Z-order control
- Input interception
- Blur/dim background
- Center overlay content

---

## Visual Design

### Background Treatment

- Blur effect applied to underlying UI
- Optional dim overlay

### Overlay Container

- Bordered panel
- Drop shadow
- Centered in window
- No OS window chrome

---

## Layout Rules

### Positioning

- Always centered (horizontal + vertical)

### Size Constraints

- Clamped to window bounds
- Maintains safe margins

### Adaptive Behavior

- Shrinks when window is smaller
- Expands up to preferred size

### Internal Structure

```
Header (fixed)
Scrollable Content
Footer (actions)
```

---

## Resizing Behavior

- Window resizing is allowed
- Overlay dynamically adjusts
- Overlay remains centered
- Content becomes scrollable when constrained

---

## Edge Case Handling

### Window Too Small

- Overlay clamps to available size
- Content scrolls internally
- Action buttons remain accessible

### Extremely Small Window

- Overlay fills most of available space
- Layout degrades gracefully
- Interaction remains functional

---

## Interaction Rules

### User Can

- Interact with overlay
- Resize window

### User Cannot

- Interact with underlying UI

---

## Focus and Input

- Focus is trapped within overlay
- OverlayHost intercepts all input
- ESC key cancels interaction

---

## UX Principles

- Context preserved (background visible)
- Interaction isolated
- Result-driven flow
- Declarative UX pattern

Overlay patterns enhance user experience by presenting functionality on top of existing content without forcing navigation away citeturn20search9.

They are commonly used to focus user attention on a specific task while blocking interaction with the rest of the interface citeturn20search8.

---

## Summary

The Interaction Overlay Mode provides a unified approach to transient UI interactions by:

- Replacing modal dialogs
- Supporting simple and complex workflows
- Maintaining architectural separation
- Ensuring consistent and modern UX behavior

This design is fully aligned with Mode-based UX architecture and supports future extensibility through stacking, adaptive layout, and strong separation of concerns.
