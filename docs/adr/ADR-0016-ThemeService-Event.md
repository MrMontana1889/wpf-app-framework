## Architecture Decision Record (ADR)

### ADR Number

ADR-0016

### Title

Introduce ThemeChanged Event in IThemeService for Decoupled Theming

### Status

- Accepted — May 2026

### Context

The existing theming system in wpf-app-framework applied themes directly through ThemeManager by merging resource dictionaries into Application.Current.Resources. While this successfully changed the active theme, it provided no mechanism for application-level components to react to theme changes.

As the system evolved, feature-specific styling (e.g., category and account UI elements) began to appear within framework theme dictionaries, creating tight coupling between framework infrastructure and application concerns. This approach was not sustainable and violated separation of concerns.

Additionally, theming is a cross-cutting concern that impacts multiple layers of the application. Without a defined extension point, application-level composition (such as merging feature-specific resource dictionaries) could not be performed cleanly.

Key constraints:
- The framework must remain independent of application-specific styling
- Existing ThemeManager behavior must remain stable
- The solution must support runtime theme switching
- The system must avoid introducing tight coupling between framework and app layers

### Decision

Extend IThemeService to introduce an event-driven notification model for theme changes.

Specifically:

- Add CurrentTheme property:
  - Represents the resolved active theme (Light or Dark)

- Add ThemeChanged event:
  - EventHandler<ThemeChangedEventArgs>
  - Raised after a theme is applied

- Introduce ThemeChangedEventArgs:
  - Carries the resolved theme value (never "System")

- Update ThemeService behavior:
  - Resolve "System" to a concrete theme before publishing state/event
  - Update CurrentTheme after applying theme
  - Raise ThemeChanged event after successful application

- Maintain existing ThemeManager responsibilities:
  - Continue handling resource dictionary merging
  - No changes to ThemeManager implementation

- Enforce architectural boundary:
  - Framework MUST NOT manage application resource dictionaries
  - Application MAY subscribe to ThemeChanged to apply its own resources

This establishes a publish-subscribe style interaction model for theme changes.

### Rationale

This decision introduces a clean extension point while preserving existing functionality.

Event-driven design promotes loose coupling between components, allowing producers and consumers to evolve independently without direct dependencies. citeturn33search51

By emitting a ThemeChanged event, the framework acts as a producer of state changes, while applications can act as consumers. This aligns with modern event-driven architecture principles, where components react to changes rather than being tightly bound together. citeturn33search52

Key benefits:
- Maintains strict separation between framework and application responsibilities
- Enables composable theming without modifying framework code
- Improves extensibility for future features (e.g., feature-specific themes)
- Reduces risk of theme-related regressions by centralizing control

This approach also prevents further leakage of application-specific styling into framework resources, which had already begun to degrade maintainability.

### Alternatives Considered

#### 1. Keep current implementation (no event)

- Pros:
  - No changes required
  - Simple design
- Cons:
  - No extension point for applications
  - Forces framework to own feature styling
  - Leads to tight coupling and poor scalability

**Rejected** due to lack of extensibility and long-term maintainability issues.

#### 2. Move all theming logic into the application

- Pros:
  - Complete application control
  - No framework complexity
- Cons:
  - Duplicates logic across apps
  - Loses shared theming infrastructure
  - Breaks reuse model of wpf-app-framework

**Rejected** because it weakens the purpose of the framework.

#### 3. Introduce direct framework hooks for app dictionaries

- Pros:
  - Centralized control of all theming
- Cons:
  - Tight coupling between framework and app
  - Violates separation of concerns
  - Difficult to scale across multiple apps

**Rejected** due to architectural coupling.

### Consequences

#### Positive

- Establishes a clean, extensible theming boundary
- Enables application-level resource composition
- Prevents framework pollution with feature-specific styles
- Supports runtime theme changes and future customization
- Improves long-term maintainability and scalability

#### Negative

- Introduces additional responsibility for application layer
- Requires applications to manage their own resource dictionary merging
- Adds slight complexity to theming workflow

### Follow-Up Work

- Phase 2: Move feature-specific brushes to application-level dictionaries
- Implement application subscription to ThemeChanged event
- Add application-level Light/Dark theme dictionaries
- Validate behavior using Dev.Wpf.TestHost
- Write ADR documenting application-level theming structure (if needed)

### Constraints and Rules (Optional)

- ThemeService MUST NOT manage application resource dictionaries
- ThemeManager MUST remain unchanged
- ThemeChanged event MUST emit resolved theme only (Light/Dark)
- Application-level theming MUST be implemented outside the framework

### References

- docs/adr/adr-0002-application-layer.md
- docs/adr/adr-0003-operation-model.md

### Notes (Optional)

This decision formalizes theming as an architectural concern rather than a purely UI concern. It ensures that future features can extend theming without modifying framework internals.

### Summary

Introduce a ThemeChanged event in IThemeService to enable decoupled, event-driven theming. This establishes a clean boundary between framework theming infrastructure and application-level styling, improving maintainability, extensibility, and architectural alignment.
