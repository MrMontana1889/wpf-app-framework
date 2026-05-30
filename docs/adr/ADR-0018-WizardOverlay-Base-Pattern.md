## Architecture Decision Record (ADR)

### ADR Number

ADR-0018

### Title

Establish WizardOverlay Base Pattern for Multi-Step Interaction Flows

### Status

- Accepted — May 2026

---

### Context

Following the introduction of the Interaction Overlay System (ADR-0017), the system now supports both simple interactions (e.g., confirm dialogs) and complex, stateful workflows (e.g., multi-step wizards).

During Phase 11 validation, a 3-step wizard overlay was implemented in Dev.Wpf.TestHost to test the system's ability to support:

- Multi-step navigation
- Stateful data collection
- Layout stability under resizing
- Scroll behavior within constrained containers
- Result propagation via overlay completion

The implementation demonstrated that the overlay system is capable of hosting complex workflows successfully. However, the wizard implementation also revealed a recurring pattern that is likely to be reused across multiple features:

- Step-based navigation (Back / Next / Finish)
- Centralized shared data model
- Step-dependent UI composition
- Command-driven enable/disable logic
- Final result aggregation

Without a defined pattern, future wizard implementations risk:

- Inconsistent navigation behavior
- Repeated boilerplate logic
- Divergent UX patterns across features
- Increased maintenance cost

A standardized pattern is required to ensure consistency, reuse, and correctness.

---

### Decision

Establish a **WizardOverlay base pattern** for implementing multi-step interaction flows within the overlay system.

This pattern defines a consistent structure for wizard-style overlays.

---

#### Core Responsibilities

Wizard overlays MUST:

- Implement `IInteractionOverlay<TResult>`
- Maintain a step index (`StepIndex`) representing the current step
- Maintain a single shared data object (`Data`) for all steps
- Provide navigation commands:
  - `NextCommand`
  - `BackCommand`
  - `FinishCommand`
  - `CancelCommand`
- Return result via overlay callback on completion

---

#### Navigation Model

Wizard overlays MUST:

- Use a bounded `StepIndex` (e.g., 0 → N-1)
- Enforce navigation rules via command logic:
  - Back disabled on first step
  - Next disabled or hidden on last step
  - Finish enabled only on final step (or based on state)

Navigation behavior MUST be driven by state, not view logic.

---

#### State Management

Wizard overlays MUST:

- Store all user input in a single shared data object
- Use data binding to persist state across steps
- Avoid per-step data duplication or reset behavior

---

#### UI Composition

Wizard UI MUST:

- Be defined via DataTemplates
- Use a single overlay root (`OverlayRootPanel`)
- Use a container that defines layout constraints (Width, MinHeight, MaxHeight)

Step content MUST:

- Be switched based on `StepIndex` using:
  - DataTriggers
  - ContentTemplate switching
- NOT be implemented as separate windows or dialogs

---

#### Layout Rules

Wizard overlays SHOULD:

- Use a consistent container size (e.g., Width + MinHeight)
- Prevent layout shifting between steps
- Support resizing gracefully
- Support scrolling for overflow content using `ScrollViewer`

Container MUST define size; content MUST adapt to container.

---

#### Command Behavior

Commands MUST:

- Reflect correct `CanExecute` state based on `StepIndex`
- Update UI enable state automatically via binding
- Avoid hard-coded UI logic

---

#### Result Handling

Wizard overlays MUST:

- Return a complete data object via callback on Finish
- Support Cancel behavior (ESC or Cancel button)
- Optionally differentiate between Cancel and Finish in the future

---

### Rationale

This decision captures a validated, working interaction pattern and promotes it to a reusable standard.

#### Consistency

- Ensures all wizard-style interactions behave predictably
- Standardizes navigation, layout, and interaction patterns

#### Reusability

- Reduces boilerplate across features
- Enables rapid implementation of new workflows

#### Maintainability

- Centralizes common logic patterns (step state, navigation)
- Reduces duplication and divergence

#### UX Quality

- Ensures consistent behavior:
  - Navigation patterns
  - Layout stability
  - Keyboard behavior
- Avoids step-dependent layout shifting

#### Architectural Alignment

- Keeps interaction logic in overlays (Dev.Core)
- Keeps UI composition in DataTemplates (Dev.Wpf)
- Preserves separation of concerns

---

### Alternatives Considered

#### 1. Implement each wizard ad-hoc

**Pros**
- Flexible per use case
- Minimal upfront abstraction

**Cons**
- Duplicate logic across features
- Inconsistent behavior and UX
- Higher maintenance cost

**Reason Not Selected**
Leads to fragmentation and inconsistency.

---

#### 2. Introduce a full Wizard Framework abstraction

**Pros**
- Maximum reuse
- Centralized control

**Cons**
- Over-engineering for current scope
- Reduced flexibility for custom workflows
- Increased complexity

**Reason Not Selected**
Premature abstraction; base pattern is sufficient.

---

#### 3. Use separate windows for wizard flows

**Pros**
- Built-in modal behavior

**Cons**
- Breaks overlay system consistency
- Poor integration with interaction framework
- Inconsistent UX

**Reason Not Selected**
Conflicts with overlay-based architecture.

---

### Consequences

#### Positive

- Consistent wizard implementation pattern
- Reduced duplication and faster development
- Improved UX consistency across workflows
- Clear guidance for future implementations
- Aligns with validated working design

---

#### Negative

- Introduces expectations for structure and naming
- Mild constraint on how wizard flows are implemented
- Pattern may require evolution as complexity increases

---

### Follow-Up Work

- Consider extracting a reusable `WizardOverlayBase<T>` in the future
- Add optional validation patterns (per-step validation)
- Introduce step indicators (UI enhancement)
- Add distinction between Cancel and Finish outcomes
- Expand automated testing around wizard flows

---

### Constraints and Rules (Optional)

- Wizard overlays MUST use a single shared data model
- Wizard UI MUST be template-driven (no separate windows)
- Navigation MUST be state-driven (StepIndex)
- Layout MUST be container-driven (MinHeight / sizing rules)
- OverlayRootPanel MUST be the root element
- Background interaction MUST remain disabled
- Light-dismiss behavior MUST NOT be used

---

### References

- docs/adr/adr-0017-interaction-overlay-system.md
- Dev.Wpf.TestHost wizard implementation
- OverlayRootPanel and OverlayHostControl

---

### Summary

Establish a standard WizardOverlay base pattern for implementing multi-step workflows within the interaction overlay system.  

This pattern ensures consistent navigation, state management, layout behavior, and result handling, enabling scalable and maintainable development of complex UI workflows.