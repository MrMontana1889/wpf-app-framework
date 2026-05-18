# Architecture Overview

## Purpose

Brief description of the system and its goals.

---

## High-Level Structure

Describe the major components or layers.

Example:
- Core (non-UI logic)
- UI layer
- Services
- External integrations

---

## Key Architectural Principles

List rules that guide the system:

- Separation of concerns
- UI vs non-UI boundaries
- Dependency direction
- Testability expectations

---

## Component Responsibilities

Describe what each major component or project is responsible for.

Keep it high level.

---

## Dependency Rules

Define allowed relationships between components.

Example:
- Core does not reference UI
- UI may reference Core
- No circular dependencies

---

## Data Flow (Optional)

Describe how data moves through the system.

---

## Related Documentation

### ADRs
- `docs/adr/...`

### Design Docs
- `docs/design/...`

---

## Notes

Anything else useful for orientation.

---

## Maintenance

This document is a **living document** and should be updated as the system evolves.