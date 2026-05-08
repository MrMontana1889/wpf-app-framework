# Copilot Instructions for wpf-app-framework

These instructions guide GitHub Copilot and Copilot Agents when generating code, refactoring, organizing files, or performing repository‑wide operations in the **wpf-app-framework** repository.

The goal of this repository is to provide a **reusable, application‑agnostic WPF application framework** with clear architectural boundaries, strong testability, and long‑term maintainability.

---

## 📘 Repository Overview

**wpf-app-framework** is a shared WPF desktop framework targeting modern .NET:

- Non‑UI infrastructure targets **net10.0**
- WPF infrastructure targets **net10.0-windows**
- Designed for reuse across multiple desktop applications
- Consumed as source (currently via Git submodules)
- Architectural intent encoded via ADRs

This repository is **not an application** and must not contain application‑specific workflows, domain models, or persistence policy.

---

## 📁 Repository Structure

```
/src
  /Dev.Core        # Platform-agnostic application infrastructure (no WPF)
  /Dev.Wpf         # Reusable WPF UI infrastructure built on Dev.Core
/tests
  /Dev.Core.Tests  # Unit tests for Dev.Core
  /Dev.Wpf.Tests   # Unit and limited UI tests for Dev.Wpf
/docs
  /adr             # Architecture Decision Records (authoritative)
```

### Repository Rules

- All framework code must live under `/src`
- All tests must live under `/tests` and mirror the target assembly
- ADRs must live under `/docs/adr`
- Applications must not be added to this repository

---

## 🧱 Architectural Boundaries

### Dev.Core

Dev.Core is a **platform‑agnostic** application infrastructure library targeting `net10.0`.

It is responsible for:

- Application- and session-scoped infrastructure
- View-model base types and utilities
- Cross-cutting UX models and abstractions
- Service interfaces consumed by UI layers

Dev.Core MUST:
- Remain free of any WPF, UI, or Windows-specific dependencies
- Be fully unit‑testable without a UI runtime
- Avoid static mutable state
- Contain no application or domain-specific logic

Dev.Core MUST NOT:
- Reference WPF assemblies
- Encode persistence semantics or platform mechanics
- Contain UI controls, XAML, or visual state

---

### Dev.Wpf

Dev.Wpf is a **WPF-specific UI infrastructure** library targeting `net10.0-windows`.

It is responsible for:

- Shared WPF controls and control infrastructure
- Control templates and styling infrastructure
- Behaviors, converters, and attached logic
- Tree view infrastructure and related selection/state coordination
- Toolbar visibility and command routing infrastructure
- Output and message presentation infrastructure
- Mode-based UX infrastructure (partial view swapping within a stable application shell)
- Implementations of Dev.Core service abstractions for WPF

Dev.Wpf MUST:
- Reference Dev.Core only
- Contain no application workflows or business rules
- Act as a reusable UI substrate for multiple applications

Dev.Wpf MUST NOT:
- Contain persistence policy or domain logic
- Reference application or domain assemblies

---

## ✅ Allowed References

- Dev.Wpf → Dev.Core
- Dev.Core.Tests → Dev.Core
- Dev.Wpf.Tests → Dev.Wpf, Dev.Core
- Applications → Dev.Core, Dev.Wpf (outside this repo)

### ❌ Forbidden References

- Dev.Core → Dev.Wpf
- Any project → WinForms APIs
- Any project → application or domain layers

---

## ⚙ Technology Requirements

When Copilot generates code, it MUST adhere to the following:

### ✔ Required

- `net10.0` for non‑UI projects
- `net10.0-windows` for WPF projects
- CommunityToolkit.Mvvm (usage permitted, but framework intent must not depend on MVVM identity)
- Microsoft.Extensions.DependencyInjection
- Dependency Injection for all services
- async/await patterns
- File‑scoped namespaces
- Nullable reference types enabled

### ❌ Forbidden

- WinForms APIs
- Legacy MVVM frameworks (Prism, MVVM Light)
- Static global state
- Business logic in code-behind
- Application-specific workflows

---

## 🎨 WPF Rules

When generating WPF code:

- XAML bindings are required; avoid logic in code‑behind
- Controls, behaviors, and templates belong in Dev.Wpf
- All control templates and styles must be placed in `Dev.Wpf/Themes/Generic.xaml`
- Reusable UI logic must not assume a specific application

---

## 🧪 Testing Requirements

All tests must:

- Use **NUnit** (versions managed centrally when applicable)
- Follow Arrange / Act / Assert
- Favor deterministic, non‑UI tests
- Limit UI tests to control template and behavior validation
- Avoid UI automation or end‑to‑end testing

### Test Placement

- Dev.Core tests → `/tests/Dev.Core.Tests`
- Dev.Wpf tests → `/tests/Dev.Wpf.Tests`

---

## 📐 Code Style Guidelines

- One class per file
- PascalCase for types, camelCase for fields
- Prefer interfaces for services
- Avoid large static helper classes
- Namespace must reflect folder structure
- Use `ArgumentNullException.ThrowIfNull(...)` for null guards

---

## 📝 Commit Guidelines

Copilot‑generated commit messages must:

- Use **present tense** (e.g., “Add tree view selection behavior”)
- Describe *what* changed, not how
- Group related changes together
- Avoid churn or opportunistic refactoring

---

## 📚 ADR Authority

All architectural decisions for this framework are documented under `/docs/adr`.

Copilot must treat ADRs as **authoritative and binding**. When in doubt, Copilot must prefer:

> **explicit architectural boundaries → testability → maintainability → future evolution**

---

## 🤖 When in Doubt

Copilot must always choose:

**clean architecture → clear boundaries → reusable infrastructure → long‑term clarity**

over shortcuts, application‑specific assumptions, or legacy compatibility.
