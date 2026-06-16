# CTI.AIDev.Tools — Design Specification

The `CTI.AIDev.Tools` subsystem defines the tool abstraction layer used by agents to interact with the environment. Tools encapsulate all side‑effecting operations such as file editing, code generation, static analysis, build execution, and domain‑specific actions. It is split into two projects:

- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Tools`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Tools.Abstractions

This project defines all tool‑related interfaces and contracts. It contains no implementations and no references to runtime, memory, or external libraries.

### CTI.AIDev.Tools

This project contains the concrete implementation of tools. It depends on:

- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.Memory.Abstractions` (optional, depending on tool type)

It does not depend on any implementation project.

---

## Purpose

Tools provide the mechanism by which agents perform actions. They encapsulate all operations that interact with:

- The filesystem  
- The codebase  
- Build systems  
- Test runners  
- Static analysis engines  
- Domain‑specific systems  
- External processes  

Tools allow agents to act safely, consistently, and within the constraints defined by profiles.

---

## Responsibilities

The tools subsystem is responsible for:

- Defining tool interfaces and contracts.
- Implementing concrete tools for common development tasks.
- Enforcing profile‑based constraints on tool usage.
- Providing safe, controlled access to the environment.
- Providing structured request/response interfaces for tool operations.
- Allowing agents to call tools through a unified abstraction layer.

---

## Non‑responsibilities

The tools subsystem does not:

- Execute inference or interact with devices.
- Implement agent logic or planning.
- Implement memory storage or retrieval.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* or runtime implementations directly.

These concerns belong to other layers.

---

## CTI.AIDev.Tools.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Tools.Abstractions`.

### Tool entry point

#### ITool

Represents a tool that can be invoked by an agent.

```csharp
public interface ITool
{
    string Id { get; }
    string Name { get; }
    IToolKind Kind { get; }

    Task<IToolResult> ExecuteAsync(
        IToolRequest request,
        CancellationToken cancellationToken = default);
}
```

#### IToolKind

Represents the discriminator for tool type.

```csharp
public interface IToolKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Tools`):

- `FileSystemToolKind : IToolKind`
- `CodeEditingToolKind : IToolKind`
- `BuildToolKind : IToolKind`
- `AnalysisToolKind : IToolKind`

---

### Tool requests and results

#### IToolRequest

Represents a request to execute a tool.

```csharp
public interface IToolRequest
{
    string ToolId { get; }
    string ProjectId { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

#### IToolResult

Represents the result of executing a tool.

```csharp
public interface IToolResult
{
    bool Success { get; }
    string Message { get; }
    IReadOnlyDictionary<string, string> Output { get; }
}
```

---

### Tool catalog

#### IToolCatalog

Provides access to available tools.

```csharp
public interface IToolCatalog
{
    Task<IReadOnlyList<ITool>> GetAllToolsAsync(
        CancellationToken cancellationToken = default);

    Task<ITool?> GetToolByIdAsync(
        string toolId,
        CancellationToken cancellationToken = default);
}
```

---

### Tool constraints

#### IToolConstraint

Represents a constraint on tool usage.

```csharp
public interface IToolConstraint
{
    string ConstraintId { get; }
    string Description { get; }
    IToolConstraintKind Kind { get; }
}
```

#### IToolConstraintKind

Represents the discriminator for constraint type.

```csharp
public interface IToolConstraintKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Tools`):

- `ReadOnlyConstraintKind : IToolConstraintKind`
- `RestrictedPathConstraintKind : IToolConstraintKind`
- `ProfileRuleConstraintKind : IToolConstraintKind`

---

## CTI.AIDev.Tools — implementation

The `CTI.AIDev.Tools` project implements all interfaces defined in `CTI.AIDev.Tools.Abstractions`.

### Internal responsibilities

- Implementing concrete tools (file editing, code generation, build, test, analysis).
- Enforcing profile‑based constraints using `IProfileService`.
- Integrating with `IRuntimeModelHost` for AI‑powered tools (e.g., code analysis).
- Integrating with `IMemoryService` for tools that store or retrieve knowledge.
- Providing safe wrappers around filesystem and process operations.
- Ensuring all side effects are controlled and auditable.

### Internal dependencies

- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.Memory.Abstractions` (optional)

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.Tools.Abstractions`:

- **Agents** call tools to perform actions.
- **Profiles** define constraints that tools must enforce.
- **Memory** may be used by tools to store or retrieve knowledge.
- **Orchestration** selects tools for workflows.
- **UI** may display tool results but never depends on implementation.

No higher layer references `CTI.AIDev.Tools` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.Tools` is wired using constructor injection:

- A composition root creates concrete implementations of `ITool`, `IToolCatalog`, and related interfaces.
- Interfaces from `CTI.AIDev.Tools.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Tools.Tests` project:

- Tests concrete tool implementations in `CTI.AIDev.Tools`.
- Uses in‑memory or sandboxed environments for filesystem and process operations.
- Uses mocked `IRuntimeModelHost` for AI‑powered tools.
- Uses mocked `IProfileService` to test constraint enforcement.
- Verifies correct behavior for tool request/response flows.

Higher‑level tests mock `ITool` and `IToolCatalog` without referencing concrete implementations.

---

## Future extensions

The tools subsystem supports:

- Additional tool kinds via new `IToolKind` implementations.
- Additional constraint kinds via new `IToolConstraintKind` implementations.
- Domain‑specific tools (e.g., telemetry, simulation, diagnostics).
- AI‑powered tools that use runtime models for analysis or generation.
- Tool chaining and composite tools (implemented internally).

The abstractions in `CTI.AIDev.Tools.Abstractions` remain stable as the system evolves.
