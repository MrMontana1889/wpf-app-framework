## Architecture Decision Record (ADR)

### ADR Number

ADR-0017

### Title

Introduce Interaction Overlay System for Modal and Workflow UI

### Status

- Accepted — May 2026

---

### Context

The application requires a consistent and reusable way to present modal UI interactions, ranging from simple confirmations to complex multi-step workflows (e.g., wizards).

Historically, modal interactions are often implemented using:
- WPF Window dialogs
- Ad-hoc UserControl overlays
- Custom per-feature implementations

These approaches introduce several issues:
- Inconsistent UX and behavior
- Fragmented patterns across features
- Poor testability and reuse
- Difficulties handling complex interaction flows (multi-step, stateful UIs)
- Frequent focus, input, and modality bugs

At the same time, modern application requirements include:
- Hosting complex workflows (e.g., wizards)
- Supporting rich, responsive layouts with resizing and scroll
- Enforcing strict modality (blocking background interaction and focus)
- Providing consistent keyboard interaction (ESC, Enter, Tab)
- Maintaining architectural separation between UI and application logic

Constraints and considerations:
- Platform is WPF on .NET 8 (Windows-only)
- Existing architecture emphasizes separation of concerns (Dev.Core vs Dev.Wpf)
- UI logic must not leak into core application logic
- Modal interactions must support both simple and complex flows
- Overlay system must be reusable across features and testable in isolation

A unified, framework-level solution is required to address these needs.

---

### Decision

Introduce a reusable **Interaction Overlay System** to manage all modal and workflow-based UI interactions.

The system consists of:

#### Core Contract (Dev.Core)

- Define `IInteractionOverlay<TResult>`:
  - Represents an interaction that produces a result
  - Provides:
    - Result callback (`SetResultCallback`)
    - Lifecycle methods (`OnEnter`, `OnExit`)
    - Cancel behavior via `CancelCommand`

- Overlays:
  - MUST be UI-agnostic
  - MUST encapsulate interaction state and logic
  - MUST NOT reference WPF-specific types

---

#### Orchestration (ModeService)

- Introduce a service responsible for:
  - Showing overlays
  - Managing overlay lifecycle
  - Delivering results via callback
- Behavior:
  - Only one active overlay at a time
  - Overlay activation triggers UI projection
  - Overlay completion resolves interaction and exits mode

---

#### UI Projection Layer (Dev.Wpf)

- Introduce `OverlayHostControl`:
  - Projects active overlay into visual tree
  - Renders overlay via DataTemplates
  - Provides:
    - Blocking layer
    - Dim background
    - Overlay content presentation

- Introduce `OverlayRootPanel`:
  - Serves as the root container for all overlays
  - Responsibilities:
    - Keyboard interaction (ESC handling)
    - Focus management
    - Focus containment (Tab cycle)
    - Initial focus assignment on activation

---

#### Modal Enforcement

- Modal behavior is enforced through:
  - Input blocking layer (hit test)
  - Background disabling (`IsEnabled = false`)
  - Focus isolation (FocusScope + navigation cycle)
  - Lifecycle-correct focus assignment

- The system MUST:
  - Prevent background interaction
  - Prevent background focus
  - Prevent accidental dismissal (no light dismiss)

---

#### UI Composition

- Overlay UI is defined entirely via DataTemplates
- Overlay system:
  - MUST NOT contain business UI logic
  - MUST remain a projection-only layer

---

#### Supported Interaction Types

The system MUST support:

- Simple dialogs (e.g., Confirm overlay)
- Complex workflows (e.g., multi-step wizard)

---

### Rationale

This decision establishes a unified, consistent, and scalable approach to modal interactions.

Key benefits:

#### Maintainability

- Centralized interaction model reduces duplication
- Consistent patterns across all features
- Easier reasoning about modal behavior

#### Reusability

- Overlay implementations are portable and testable
- UI is decoupled from interaction logic
- DataTemplates enable flexible rendering

#### Testability

- Core interaction logic exists in non-UI layer
- Overlay behavior can be validated via test host
- Clear separation of responsibilities

#### UX Consistency

- Consistent keyboard behavior:
  - ESC → cancel
  - Enter → default action
  - Tab → contained navigation
- True modal behavior enforced across application

#### Scalability

- Supports both simple and complex interactions
- Validated with multi-step wizard overlay
- Handles resizing, scrolling, and dynamic content

---

### Alternatives Considered

#### 1. Native WPF Window dialogs

**Pros**
- Built-in modal behavior
- Familiar pattern

**Cons**
- Poor integration with application shell
- Limited customization for complex flows
- Hard to reuse and test
- Inconsistent styling and UX

**Reason Not Selected**
Does not support modern workflow needs or architectural goals.

---

#### 2. Ad-hoc UserControl overlays per feature

**Pros**
- Flexible
- Simple to implement initially

**Cons**
- Highly inconsistent behavior
- Repeated logic across features
- Difficult to maintain and extend

**Reason Not Selected**
Leads to fragmentation and long-term maintenance issues.

---

#### 3. Framework-level modal service without UI abstraction

**Pros**
- Centralized logic

**Cons**
- Tight coupling to UI
- Difficult to extend for complex interactions
- Poor separation of concerns

**Reason Not Selected**
Does not align with architectural layering principles.

---

### Consequences

#### Positive

- Unified modal interaction framework
- Consistent UX across all overlays
- Supports complex workflows (e.g., wizard)
- Strong separation of concerns
- Reliable focus and modality handling
- Reusable and extensible design

---

#### Negative

- Initial implementation complexity
- Requires adherence to overlay patterns
- Slightly increased abstraction compared to simple dialogs
- Requires discipline to maintain boundaries (Core vs WPF)

---

### Follow-Up Work

- Add validation patterns for wizard flows (future phase)
- Introduce UX polish:
  - Step indicators
  - Refined navigation patterns
- Consider animation support (optional phase)
- Expand test coverage for interaction flows
- Potential extraction into reusable shared library

---

### Constraints and Rules (Optional)

- Overlays MUST implement `IInteractionOverlay<TResult>`
- Overlays MUST NOT depend on WPF types
- Overlay UI MUST be defined via DataTemplates
- OverlayHostControl MUST NOT contain business logic
- Only one active overlay is allowed at a time
- Background UI MUST be disabled during active overlay
- OverlayRootPanel MUST manage focus and keyboard behavior
- Light-dismiss behavior (click outside to close) is prohibited

---

### References

- docs/adr/ADR-0015-Mode-Based-UX-Framework.md
- docs/design/interaction-overlay-system.md
- Dev.Wpf.TestHost (wizard and confirm overlay implementations)

---

### Summary

Introduce a reusable interaction overlay system to provide a consistent, scalable, and architecturally sound approach to modal UI and workflow interactions in WPF.  

The system separates interaction logic from UI, enforces strict modal behavior, and supports both simple dialogs and complex multi-step workflows, enabling a reliable foundation for future application features.