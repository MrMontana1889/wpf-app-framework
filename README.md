# WPF App Framework

A reusable, application-agnostic **WPF application framework** targeting modern .NET (`net10.0` / `net10.0-windows`).

This repository provides shared desktop UI infrastructure intended to be consumed by multiple WPF applications. It focuses on **application structure, session-oriented UX, and reusable UI subsystems**, while deliberately avoiding application-specific domain or workflow logic.

---

## Purpose

The goal of this framework is to provide a stable, well-defined foundation for building rich WPF desktop applications without duplicating common infrastructure across projects.

Specifically, this framework:

- Establishes clear architectural boundaries between application logic and UI mechanics
- Provides reusable UX subsystems that are expensive or error-prone to re‑implement
- Encodes architectural intent via ADRs rather than implicit convention
- Enables multiple applications to share a consistent UX foundation without shared application logic

---

## Project Structure

The framework is composed of two primary libraries:

### Dev.Core

**Target:** `net10.0`

Dev.Core is a platform-agnostic application support library. It contains no WPF dependencies and is fully unit-testable without a UI runtime.

Responsibilities include:

- Application- and session-scoped infrastructure
- View‑model base types and utilities
- Cross‑cutting UX models and abstractions
- Service interfaces consumed by UI layers

Dev.Core intentionally contains **no UI logic, no WPF types, and no application domain logic**.

---

### Dev.Wpf

**Target:** `net10.0-windows`

Dev.Wpf provides reusable WPF-specific UI infrastructure that builds on Dev.Core abstractions.

Responsibilities include:

- Shared WPF controls and control infrastructure
- Tree view system and related selection/state handling
- Toolbar visibility and command routing infrastructure
- Output and message presentation infrastructure
- Mode-based UX support (partial view swapping within a stable application shell)
- Implementations of Dev.Core services for WPF

Dev.Wpf contains **no application workflows, business rules, or persistence policy**.

---

## Architectural Principles

This framework follows a small number of non-negotiable principles:

- **Intent over implementation** — Names and structure encode *what* the system is, not *how* it is currently implemented
- **Explicit boundaries** — UI mechanics are separated from application orchestration and domain logic
- **Session-oriented UX** — User interactions are scoped to explicit application sessions
- **Replaceable infrastructure** — Applications depend on framework abstractions, not concrete mechanisms
- **ADR-driven design** — Architectural decisions are documented and enforced

---

## What This Framework Is Not

This repository intentionally does **not**:

- Contain application-specific workflows
- Contain domain models or business rules
- Define persistence semantics or data models
- Provide a control gallery or general UI toolkit
- Bind consuming applications to a specific MVVM framework identity

---

## ADRs

Architectural decisions for this framework are documented in the `/docs/adr` directory.

These ADRs define:

- Project responsibilities and dependency rules
- UX subsystem architecture (tree views, modes, output, prompts)
- Framework guarantees and non-guarantees

Consumers of this framework are expected to treat these ADRs as authoritative.

---

## Usage

This framework is consumed as **source**, currently via Git submodules, by WPF desktop applications.

It is intentionally **not yet packaged** as a NuGet dependency. Once APIs stabilize, packaging may be evaluated.

### Submodule Configuration

Because Dev.Wpf references PropertyTools, you should add a submodule both wpf-app-framework AND PropertyTools. 
They should both go in the submodules folder in the root of your repo.

For wpf-app-framework, you should pin to the `bba/ref-path` branch.
For PropertyTools, use the fork at `https://github.com/PondPackMan2023/PropertyTools` and pin to the `bba/targetframework` branch.

> **_NOTE:_** If you decide to use the PropertyTools repo directly, make sure you update the target framework to use *just* net10.
Using the multitargeting of the PropertyTools and PropertyTools.Wpf projects appears to cause some difficulties with Dev.Wpf.

---

## Consumers

Known consumers:

- *BentleyBuildApp.Next*
- *PersonalLedger*

Additional applications may consume this framework provided they respect its architectural boundaries.

---

## Status

Active development. APIs and structure may evolve as additional applications adopt the framework.

Breaking changes are expected until a formal versioning strategy is established.

---

## License

[MIT License](LICENSE)

---

## Notes

This repository was extracted from an application codebase once the shared infrastructure matured into a reusable framework. Commit history intentionally begins at the point of extraction to reflect independent ownership and intent.
