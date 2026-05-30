## Architecture Decision Record (ADR)

### ADR Number

ADR-0019

### Title

Enable Projection-Agnostic Prompt Rendering via Shared PromptView and Overlay Integration

### Status

- Accepted — May 2026

---

### Context

ADR-0006 establishes the Do Not Prompt Again Messaging System, defining the behavioral and orchestration model for prompts, including prompt identity, suppression semantics, request/response flow, and the separation of Dev.Core and Dev.Wpf responsibilities.

The original Dev.Wpf implementation provides a PromptDialog (Window-based UI) to render prompts. This approach is effective but limited to a single presentation model.

With the introduction of the Interaction Overlay System (ADR-0017), the framework now supports an alternative modal interaction model capable of hosting complex workflows and reusable UI components.

During Phase 12, the PromptDialog implementation was refactored to:

- Extract shared UI into a reusable PromptView UserControl
- Introduce a shared PromptViewModel
- Enable projection of prompts via both dialog (Window) and overlay systems
- Preserve full compatibility with ADR-0006 behavior and orchestration

This creates a requirement to formally define how prompts can be rendered through multiple UI projections without altering core behavior.

---

### Decision

Prompts SHALL support multiple rendering projections through shared UI composition while preserving the behavioral contract defined in ADR-0006.

This is achieved by:

---

#### Shared UI Composition

- Introduce `PromptView` as a reusable UserControl containing all prompt UI
- Introduce `PromptViewModel` as the shared ViewModel for all prompt rendering
- Remove UI duplication across dialog and overlay implementations

---

#### Projection Mechanisms

Prompts MAY be rendered through multiple UI hosts, including:

- Dialog projection (Window-based)
  - `PromptDialog` hosts `PromptView`
- Overlay projection (Overlay system)
  - `PromptViewModel` is rendered via DataTemplate using `OverlayRootPanel`

---

#### Presentation Strategy

- Prompt rendering continues to be delegated to `IPromptPresenter`
- The application determines which projection to use (dialog or overlay)
- The PromptOrchestrationService remains unaware of presentation details

---

#### Overlay Integration

- Overlay projection uses existing infrastructure:
  - `IModeService`
  - `OverlayRootPanel`
  - `IInteractionOverlay<TResult>`
- A lightweight adapter (e.g., `PromptOverlay`) MAY be used to bridge `PromptViewModel` into the overlay system without modifying core logic

---

#### Behavioral Preservation

All prompt behavior defined in ADR-0006 MUST remain unchanged:

- Prompt identity and suppression semantics
- Prompt request/response contract
- Suppression persistence behavior
- Dialog orchestration flow

No prompt behavior SHALL be implemented in UI layers.

---

#### UI Responsibilities

- `PromptView` MUST:
  - Be host-agnostic (no Window or overlay assumptions)
  - Define its own visual surface (background) for correct rendering across hosts
  - Use dynamic resources for theme compatibility

- Hosts (Window or Overlay) MUST:
  - Manage their own lifecycle (e.g., closing behavior)
  - Provide modal behavior guarantees

---

### Rationale

This decision extends the prompt system from a single fixed UI projection to a flexible, projection-agnostic model while preserving clean architectural boundaries.

#### Reusability

- Eliminates duplicate prompt UI
- Enables consistent rendering across multiple hosts
- Reduces maintenance overhead

#### Architectural Alignment

- Preserves strict Dev.Core / Dev.Wpf separation
- Maintains prompt behavior independence from UI
- Aligns with overlay-based interaction model (ADR-0017)

#### Flexibility

- Allows applications to choose presentation strategy
- Supports traditional dialogs and modern overlay UX
- Enables future projections (inline, embedded, etc.)

#### Consistency

- Ensures identical behavior and appearance across projection paths
- Guarantees prompt semantics remain unchanged regardless of UI host

#### Theming and Visual Correctness

- Shared PromptView ensures consistent appearance
- Dynamic resource usage enables correct rendering across light and dark themes

---

### Alternatives Considered

#### 1. Keep PromptDialog as the sole rendering mechanism

**Pros**
- Simpler implementation
- No refactoring required

**Cons**
- No support for overlay-based UX
- Duplicate UI required for alternative projections
- Limits flexibility

**Reason Not Selected**
Does not support modern interaction patterns or overlay system integration.

---

#### 2. Implement separate overlay-specific prompt UI

**Pros**
- Tailored overlay experience

**Cons**
- Duplicate UI definitions
- Risk of behavioral divergence
- Increased maintenance cost

**Reason Not Selected**
Violates DRY principles and reduces consistency.

---

#### 3. Move prompt presentation logic into Dev.Core

**Pros**
- Centralized control

**Cons**
- Breaks UI separation
- Violates ADR-0006 constraints
- Reduces flexibility

**Reason Not Selected**
Conflicts with architectural layering principles.

---

### Consequences

#### Positive

- Unified prompt UI across multiple projections
- No duplication of layout or logic
- Flexible rendering strategy per application
- Full compatibility with overlay system
- Improved long-term maintainability
- Consistent behavior and visual appearance

---

#### Negative

- Requires explicit lifecycle handling in dialog host (e.g., close signaling)
- Slight increase in architectural complexity
- Requires careful handling of host-specific concerns (e.g., background rendering)

---

### Follow-Up Work

- Provide a reference `PromptOverlayPresenter` implementation (optional)
- Evaluate replacing dialog-based prompts with overlays in selected workflows
- Extend theming support if application-specific brushes are introduced
- Consider additional projections (inline prompts, embedded workflows)

---

### Constraints and Rules (Optional)

- Prompt behavior MUST remain in Dev.Core
- PromptView MUST remain UI-only and host-agnostic
- PromptViewModel MUST be reused across all projections
- Hosts MUST manage lifecycle (close behavior, modal semantics)
- Overlay projection MUST use existing overlay infrastructure
- No duplication of prompt UI is allowed

---

### References

- docs/adr/adr-0006-do-not-prompt-again-messaging-system.md
- docs/adr/adr-0017-interaction-overlay-system.md
- docs/adr/adr-0018-wizard-overlay-base-pattern.md
- Dev.Wpf PromptView and PromptDialog implementations
- Dev.Wpf.TestHost Phase 12 validation harness

---

### Summary

This ADR introduces projection-agnostic prompt rendering by extracting shared UI and enabling prompts to be displayed through both dialog and overlay systems.  

The decision preserves all behavioral guarantees defined in ADR-0006 while enabling flexible, reusable, and consistent UI composition across multiple presentation models.