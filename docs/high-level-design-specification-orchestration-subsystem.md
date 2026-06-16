# CTI.AIDev.Orchestration — Design Specification

The `CTI.AIDev.Orchestration` subsystem coordinates agents, tools, memory, profiles, and runtime into coherent workflows. It is the top‑level execution layer that receives tasks from the UI or host application and produces structured results. It is split into two projects:

- `CTI.AIDev.Orchestration.Abstractions`
- `CTI.AIDev.Orchestration`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Orchestration.Abstractions

This project defines all orchestration‑related interfaces and contracts. It contains no implementations and no references to runtime, agents, tools, memory, or external libraries.

### CTI.AIDev.Orchestration

This project contains the concrete implementation of orchestration. It depends on:

- `CTI.AIDev.Orchestration.Abstractions`
- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`

It does not depend on any implementation project.

---

## Purpose

Orchestration is the system’s conductor. It:

- Accepts high‑level tasks from the UI or host  
- Selects the appropriate profile  
- Resolves the correct model instance  
- Selects and configures the appropriate agent  
- Coordinates multi‑step workflows  
- Aggregates results  
- Provides structured outputs to the UI  

Orchestration ensures that all subsystems work together coherently.

---

## Responsibilities

The orchestration subsystem is responsible for:

- Defining orchestration interfaces and workflow contracts.
- Accepting high‑level tasks and decomposing them into agent requests.
- Selecting profiles, agents, and tools based on task context.
- Coordinating agent execution and multi‑step workflows.
- Managing long‑running tasks and progress reporting.
- Integrating with memory for context retrieval and storage.
- Integrating with model registry for model instance resolution.
- Producing structured orchestration results for the UI.

---

## Non‑responsibilities

The orchestration subsystem does not:

- Execute inference directly.
- Execute tools directly.
- Implement memory storage or retrieval.
- Implement agent reasoning or planning.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* or runtime implementations directly.

These concerns belong to other layers.

---

## CTI.AIDev.Orchestration.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Orchestration.Abstractions`.

### Orchestration entry point

#### IOrchestrationService

Represents the primary entry point for executing high‑level tasks.

```csharp
public interface IOrchestrationService
{
    Task<IOrchestrationResult> ExecuteTaskAsync(
        IOrchestrationTask task,
        CancellationToken cancellationToken = default);

    Task<IOrchestrationStatus> GetStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);
}
```

---

### Orchestration tasks

#### IOrchestrationTask

Represents a high‑level task submitted by the UI or host.

```csharp
public interface IOrchestrationTask
{
    string TaskId { get; }
    string ProjectId { get; }
    string Description { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

---

### Orchestration results

#### IOrchestrationResult

Represents the final result of an orchestration workflow.

```csharp
public interface IOrchestrationResult
{
    string TaskId { get; }
    bool Success { get; }
    string Summary { get; }
    IReadOnlyList<IOrchestrationStepResult> Steps { get; }
}
```

#### IOrchestrationStepResult

Represents the result of a single orchestration step.

```csharp
public interface IOrchestrationStepResult
{
    string Description { get; }
    IAgentResult AgentResult { get; }
}
```

---

### Orchestration status

#### IOrchestrationStatus

Represents the status of a long‑running orchestration task.

```csharp
public interface IOrchestrationStatus
{
    string TaskId { get; }
    bool IsCompleted { get; }
    int ProgressPercent { get; }
    string CurrentStepDescription { get; }
}
```

---

### Orchestration planning

#### IOrchestrationPlanner

Determines how to break a high‑level task into agent requests.

```csharp
public interface IOrchestrationPlanner
{
    Task<IReadOnlyList<IOrchestrationPlanStep>> PlanAsync(
        IOrchestrationTask task,
        IProfile profile,
        CancellationToken cancellationToken = default);
}
```

#### IOrchestrationPlanStep

Represents a single step in an orchestration plan.

```csharp
public interface IOrchestrationPlanStep
{
    string Description { get; }
    string AgentKindName { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

---

### Agent selection

#### IAgentSelector

Selects the appropriate agent for a given orchestration plan step.

```csharp
public interface IAgentSelector
{
    Task<IAgent> SelectAgentAsync(
        IOrchestrationPlanStep step,
        IProfile profile,
        CancellationToken cancellationToken = default);
}
```

---

### Model resolution

#### IModelResolver

Resolves the correct model instance for a given task.

```csharp
public interface IModelResolver
{
    Task<string> ResolveModelInstanceIdAsync(
        IOrchestrationTask task,
        IProfile profile,
        CancellationToken cancellationToken = default);
}
```

---

## CTI.AIDev.Orchestration — implementation

The `CTI.AIDev.Orchestration` project implements all interfaces defined in `CTI.AIDev.Orchestration.Abstractions`.

### Internal responsibilities

- Implementing orchestration workflows.
- Integrating with `IProfileService` to resolve profiles.
- Integrating with `IModelRegistry` to resolve model instances.
- Integrating with `IAgentSelector` to choose agents.
- Integrating with `IAgent` to execute tasks.
- Integrating with `IMemoryService` for context retrieval and storage.
- Managing long‑running tasks and progress reporting.
- Aggregating results into `IOrchestrationResult`.

### Internal dependencies

- `CTI.AIDev.Orchestration.Abstractions`
- `CTI.AIDev.Agents.Abstractions`
- `CTI.AIDev.Tools.Abstractions`
- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.Orchestration.Abstractions`:

- **UI** submits tasks and receives structured results.
- **Agents** are invoked by orchestration.
- **Profiles** influence orchestration planning.
- **Memory** provides context for orchestration.
- **Tools** are invoked indirectly through agents.
- **Runtime** is used indirectly through agents.

No higher layer references `CTI.AIDev.Orchestration` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.Orchestration` is wired using constructor injection:

- A composition root creates concrete implementations of `IOrchestrationService`, `IOrchestrationPlanner`, `IAgentSelector`, and related interfaces.
- Interfaces from `CTI.AIDev.Orchestration.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Orchestration.Tests` project:

- Tests concrete orchestration implementations.
- Uses mocked `IAgent`, `IProfileService`, `IModelRegistry`, and `IMemoryService`.
- Verifies correct multi‑step orchestration behavior.
- Verifies correct agent selection and model resolution.
- Verifies correct progress reporting and status tracking.

Higher‑level tests mock `IOrchestrationService` without referencing concrete implementations.

---

## Future extensions

The orchestration subsystem supports:

- Additional orchestration strategies via new `IOrchestrationPlanner` implementations.
- Multi‑agent workflows and collaboration patterns.
- Long‑running background tasks.
- Task cancellation and check-pointing.
- Workflow templates and reusable orchestration patterns.

The abstractions in `CTI.AIDev.Orchestration.Abstractions` remain stable as the system evolves.
