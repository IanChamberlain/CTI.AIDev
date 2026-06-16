# CTI.AIDev.Agents — Design Specification

The `CTI.AIDev.Agents` subsystem defines the agent execution model, including planning, reasoning, tool usage, and interaction with profiles, memory, and runtime. It is split into two projects:

- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Agents`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Agents.Abstractions

This project defines all agent‑related interfaces and contracts. It contains no implementations and no references to runtime, tools, memory, or external libraries.

### CTI.AIDev.Agents

This project contains the concrete implementation of agents. It depends on:

- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`

It does not depend on any implementation project.

---

## Purpose

Agents are the reasoning and action‑taking components of the system. They:

- Interpret tasks  
- Plan multi‑step actions  
- Use tools  
- Query memory  
- Apply profile rules  
- Interact with runtime models  
- Produce structured results  

Agents are the “brains” of the system, but they do not perform side effects directly—tools do.

---

## Responsibilities

The agents subsystem is responsible for:

- Defining agent interfaces and behavior contracts.
- Implementing agent loops (plan → act → observe → refine).
- Integrating with profiles to enforce rules and constraints.
- Integrating with memory for retrieval and storage.
- Integrating with tools for environment interaction.
- Integrating with runtime for inference and reasoning.
- Producing structured agent results for orchestration.

---

## Non‑responsibilities

The agents subsystem does not:

- Execute tools directly (tools perform side effects).
- Implement memory storage or retrieval.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* or runtime implementations directly.

These concerns belong to other layers.

---

## CTI.AIDev.Agents.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Agents.Abstractions`.

### Agent entry point

#### IAgent

Represents an agent capable of performing tasks.

```csharp
public interface IAgent
{
    string Id { get; }
    string Name { get; }
    IAgentKind Kind { get; }

    Task<IAgentResult> ExecuteAsync(
        IAgentRequest request,
        CancellationToken cancellationToken = default);
}
```

#### IAgentKind

Represents the discriminator for agent type.

```csharp
public interface IAgentKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Agents`):

- `CodingAgentKind : IAgentKind`
- `RefactoringAgentKind : IAgentKind`
- `AnalysisAgentKind : IAgentKind`
- `ExplanationAgentKind : IAgentKind`

---

### Agent requests and results

#### IAgentRequest

Represents a request for an agent to perform a task.

```csharp
public interface IAgentRequest
{
    string ProjectId { get; }
    string TaskDescription { get; }
    IProfile Profile { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

#### IAgentResult

Represents the result of an agent execution.

```csharp
public interface IAgentResult
{
    bool Success { get; }
    string Summary { get; }
    IReadOnlyList<IAgentStepResult> Steps { get; }
}
```

#### IAgentStepResult

Represents the result of a single reasoning or action step.

```csharp
public interface IAgentStepResult
{
    string Description { get; }
    string Output { get; }
    IReadOnlyList<IToolResult> ToolResults { get; }
}
```

---

### Agent planning

#### IAgentPlanner

Represents a planning component used by agents.

```csharp
public interface IAgentPlanner
{
    Task<IReadOnlyList<IAgentPlanStep>> PlanAsync(
        IAgentRequest request,
        CancellationToken cancellationToken = default);
}
```

#### IAgentPlanStep

Represents a single step in an agent plan.

```csharp
public interface IAgentPlanStep
{
    string Description { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

---

### Agent reasoning

#### IAgentReasoner

Represents a reasoning component that uses runtime models.

```csharp
public interface IAgentReasoner
{
    Task<IAgentReasoningResult> ReasonAsync(
        IAgentReasoningRequest request,
        CancellationToken cancellationToken = default);
}
```

#### IAgentReasoningRequest

Represents a reasoning request.

```csharp
public interface IAgentReasoningRequest
{
    string Input { get; }
    string ProjectId { get; }
    IProfile Profile { get; }
}
```

#### IAgentReasoningResult

Represents the result of a reasoning operation.

```csharp
public interface IAgentReasoningResult
{
    string Output { get; }
    IReadOnlyList<string> Evidence { get; }
}
```

---

### Agent tool usage

#### IAgentToolSelector

Determines which tools an agent should use for a given step.

```csharp
public interface IAgentToolSelector
{
    Task<IReadOnlyList<ITool>> SelectToolsAsync(
        IAgentPlanStep step,
        IProfile profile,
        CancellationToken cancellationToken = default);
}
```

---

### Agent memory usage

#### IAgentMemoryAccessor

Provides memory access for agents.

```csharp
public interface IAgentMemoryAccessor
{
    Task<IReadOnlyList<IMemoryEntry>> RetrieveContextAsync(
        IAgentRequest request,
        CancellationToken cancellationToken = default);

    Task StoreResultAsync(
        IAgentResult result,
        CancellationToken cancellationToken = default);
}
```

---

## CTI.AIDev.Agents — implementation

The `CTI.AIDev.Agents` project implements all interfaces defined in `CTI.AIDev.Agents.Abstractions`.

### Internal responsibilities

- Implementing agent loops (plan → reason → act → store).
- Integrating with `IRuntimeModelHost` for reasoning.
- Integrating with `IToolCatalog` and `ITool` for actions.
- Integrating with `IMemoryService` for retrieval and storage.
- Integrating with `IProfileService` for rule enforcement.
- Producing structured results for orchestration.

### Internal dependencies

- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.Agents.Abstractions`:

- **Orchestration** uses agents to perform tasks.
- **Profiles** influence agent behavior.
- **Memory** provides context for agents.
- **Tools** are invoked by agents.
- **UI** displays agent results but never depends on implementation.

No higher layer references `CTI.AIDev.Agents` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.Agents` is wired using constructor injection:

- A composition root creates concrete implementations of `IAgent`, `IAgentPlanner`, `IAgentReasoner`, and related interfaces.
- Interfaces from `CTI.AIDev.Agents.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Agents.Tests` project:

- Tests concrete agent implementations in `CTI.AIDev.Agents`.
- Uses mocked `IRuntimeModelHost` for reasoning.
- Uses mocked `ITool` and `IToolCatalog` for tool usage.
- Uses mocked `IMemoryService` for context retrieval.
- Uses mocked `IProfileService` for rule enforcement.
- Verifies correct multi‑step agent behavior.

Higher‑level tests mock `IAgent` without referencing concrete implementations.

---

## Future extensions

The agents subsystem supports:

- Additional agent kinds via new `IAgentKind` implementations.
- Additional planning strategies via new `IAgentPlanner` implementations.
- Additional reasoning strategies via new `IAgentReasoner` implementations.
- Composite agents that delegate to other agents.
- Multi‑agent collaboration patterns (implemented internally).

The abstractions in `CTI.AIDev.Agents.Abstractions` remain stable as the system evolves.
