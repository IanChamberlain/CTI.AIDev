# CTI.AIDev.UI — Design Specification

The `CTI.AIDev.UI` subsystem provides the presentation and interaction layer for the system. It is the only subsystem that interacts directly with the user. It is split into two projects:

- `CTI.AIDev.UI.Abstractions`
- `CTI.AIDev.UI`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

The UI depends only on orchestration abstractions and never interacts directly with agents, tools, memory, runtime, or model registry implementations.

---

## Project structure

### CTI.AIDev.UI.Abstractions

This project defines all UI‑related interfaces and contracts. It contains no implementations and no references to orchestration, agents, tools, memory, or external libraries.

### CTI.AIDev.UI

This project contains the concrete implementation of the UI layer. It depends on:

- `CTI.AIDev.UI.Abstractions`
- `CTI.AIDev.Orchestration.Abstractions`

It does not depend on any implementation project.

---

## Purpose

The UI subsystem provides:

- A user‑facing interface for submitting tasks  
- A mechanism for displaying orchestration results  
- Real‑time status updates for long‑running tasks  
- A structured way to present agent reasoning and tool outputs  
- A boundary between the user and the internal architecture  

The UI does not perform any reasoning, planning, or execution. It is a pure presentation and interaction layer.

---

## Responsibilities

The UI subsystem is responsible for:

- Accepting user input and converting it into orchestration tasks.
- Displaying orchestration results in a structured, user‑friendly format.
- Displaying progress and status updates for long‑running tasks.
- Providing a consistent visual and interaction model across the system.
- Representing all UI‑level data using interface‑based DTOs.
- Remaining completely decoupled from internal implementation details.

---

## Non‑responsibilities

The UI subsystem does not:

- Execute inference or interact with devices.
- Implement agent logic or planning.
- Implement tools or side‑effecting operations.
- Implement memory storage or retrieval.
- Implement orchestration logic.
- Reference Microsoft.AI.Foundry.* or runtime implementations.

These concerns belong to other layers.

---

## CTI.AIDev.UI.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.UI.Abstractions`.

### UI entry point

#### IUiController

Represents the primary entry point for UI interactions.

```csharp
public interface IUiController
{
    Task<IUiTaskResponse> SubmitTaskAsync(
        IUiTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<IUiStatusResponse> GetStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);
}
```

---

### UI task request and response

#### IUiTaskRequest

Represents a user‑submitted task.

```csharp
public interface IUiTaskRequest
{
    string ProjectId { get; }
    string Description { get; }
    IReadOnlyDictionary<string, string> Parameters { get; }
}
```

#### IUiTaskResponse

Represents the response after submitting a task.

```csharp
public interface IUiTaskResponse
{
    string TaskId { get; }
    string Message { get; }
}
```

---

### UI status response

#### IUiStatusResponse

Represents the status of a long‑running task.

```csharp
public interface IUiStatusResponse
{
    string TaskId { get; }
    bool IsCompleted { get; }
    int ProgressPercent { get; }
    string CurrentStepDescription { get; }
}
```

---

### UI result presentation

#### IUiResultPresenter

Transforms orchestration results into UI‑friendly representations.

```csharp
public interface IUiResultPresenter
{
    Task<IUiRenderedResult> RenderAsync(
        IOrchestrationResult result,
        CancellationToken cancellationToken = default);
}
```

#### IUiRenderedResult

Represents a UI‑ready rendering of an orchestration result.

```csharp
public interface IUiRenderedResult
{
    string Summary { get; }
    IReadOnlyList<IUiRenderedStep> Steps { get; }
}
```

#### IUiRenderedStep

Represents a UI‑ready rendering of a single step.

```csharp
public interface IUiRenderedStep
{
    string Description { get; }
    string Output { get; }
}
```

---

## CTI.AIDev.UI — implementation

The `CTI.AIDev.UI` project implements all interfaces defined in `CTI.AIDev.UI.Abstractions`.

### Internal responsibilities

- Converting user input into `IOrchestrationTask`.
- Calling `IOrchestrationService` to execute tasks.
- Polling orchestration for status updates.
- Rendering results using `IUiResultPresenter`.
- Providing a consistent visual and interaction model.
- Ensuring no internal implementation details leak to the user.

### Internal dependencies

- `CTI.AIDev.UI.Abstractions`
- `CTI.AIDev.Orchestration.Abstractions`

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

The UI depends only on `CTI.AIDev.Orchestration.Abstractions`:

- **UI → Orchestration**: submits tasks and receives results  
- **Orchestration → Agents**: internal  
- **Orchestration → Memory**: internal  
- **Orchestration → Tools**: internal  
- **Orchestration → Runtime**: internal  

The UI never interacts with agents, tools, memory, or runtime directly.

---

## Construction and dependency injection

`CTI.AIDev.UI` is wired using constructor injection:

- A composition root creates concrete implementations of `IUiController` and `IUiResultPresenter`.
- Interfaces from `CTI.AIDev.UI.Abstractions` are registered in the DI container.
- The UI receives only interfaces in its constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.UI.Tests` project:

- Tests concrete UI implementations.
- Uses mocked `IOrchestrationService` for task execution.
- Verifies correct transformation of user input into orchestration tasks.
- Verifies correct rendering of orchestration results.
- Verifies correct status polling behavior.

Higher‑level tests mock `IUiController` without referencing concrete implementations.

---

## Future extensions

The UI subsystem supports:

- Additional rendering strategies via new `IUiResultPresenter` implementations.
- Multi‑view UI (CLI, web, IDE integration).
- Real‑time streaming of agent reasoning.
- Interactive debugging and step‑through execution.
- Customizable themes and presentation layers.

The abstractions in `CTI.AIDev.UI.Abstractions` remain stable as the system evolves.
