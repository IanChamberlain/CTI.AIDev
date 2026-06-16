# CTI.AIDev Standards — Architectural Boundaries and Dependency Inversion

This document defines the mandatory architectural rules for all CTI.AIDev projects. These rules enforce strict separation of concerns, ensure testability, and guarantee that all dependencies flow through abstractions rather than concrete implementations.

## Purpose

The purpose of this standard is to enforce a consistent architectural structure across the entire system. All boundaries between components must be defined using interfaces, and all dependencies must be inverted so that higher‑level modules depend only on abstractions. This ensures that implementations can evolve independently, testing is simplified, and architectural drift is prevented.

## Core Architectural Principles

### Principle 1: Interfaces define all boundaries

Every boundary between components, layers, or modules must be defined using interfaces. This applies to:

- Public APIs
- Internal APIs
- Cross‑project dependencies
- Cross‑layer dependencies
- Internal service boundaries
- Any class that is consumed by another class

No concrete type may be referenced across a boundary.

### Principle 2: Interfaces may only expose other interfaces or primitives

Interfaces must not expose:

- Concrete classes
- Records
- Structs
- DTOs
- Framework types (except primitives)
- Collections of concrete types

Allowed types:

- Other interfaces
- Primitive types (string, bool, numeric types)
- Nullable primitives
- Collections of interfaces (e.g., `IReadOnlyList<IMyType>`)

### Principle 3: All complex types must themselves be interfaces

If an interface requires a complex type, that type must also be an interface.

Examples:

- If `IAgent` returns a result, the result must be `IAgentResult`.
- If `IModelHost` returns metadata, the metadata must be `IInferenceMetadata`.
- If `IProfile` exposes rules, the rules must be `IProfileRule`.

### Principle 4: No concrete types cross project boundaries

Concrete types must be internal to their implementation projects.

Only interfaces cross boundaries.

### Principle 5: Every project has a corresponding `.Abstractions` project

For each project:

- `CTI.AIDev.Runtime`
- `CTI.AIDev.ModelRegistry`
- `CTI.AIDev.Profiles`
- `CTI.AIDev.Memory`
- `CTI.AIDev.Tools`
- `CTI.AIDev.Agents`
- `CTI.AIDev.Orchestration`

There must be a corresponding:

- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Orchestration.Abstractions`

### Principle 6: Implementation projects depend on abstractions, never the reverse

Example:

- `CTI.AIDev.Runtime` → depends on → `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Agents` → depends on → `CTI.AIDev.Agents.Abstractions`

Abstractions projects must never depend on implementation projects.

### Principle 7: Higher layers depend only on abstractions

Example:

`CTI.AIDev.Agents` may depend on:

- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`

It must not depend on:

- `CTI.AIDev.Runtime`
- `CTI.AIDev.Tools`
- `CTI.AIDev.Memory`
- `CTI.AIDev.Profiles`

### Principle 8: Constructor injection is mandatory

All dependencies must be provided via constructor injection.

Rules:

- No service locator
- No static access
- No ambient context
- No optional dependencies
- No property injection

Every class must declare its dependencies explicitly.

### Principle 9: Testability is a first‑class design constraint

Because all dependencies are interfaces:

- Every class can be unit tested in isolation
- All collaborators can be mocked or faked
- No test requires concrete runtime components
- No test requires Foundry Local or hardware

### Principle 10: No implementation details leak into abstractions

Interfaces must not reference:

- Foundry Local types
- ONNX Runtime types
- WinML types
- File system types
- Network types
- UI types
- Concrete DTOs
- Concrete configuration objects

Abstractions must remain pure.

## Exceptions

The following are allowed as concrete types:

- Enums (simple discriminators)
- Exceptions (internal only)

These must not appear in interface signatures.

## Enforcement

Any violation of these rules is considered an architectural error. All new APIs, interfaces, and project boundaries must be reviewed for compliance before merging.
