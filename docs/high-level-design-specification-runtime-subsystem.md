# CTI.AIDev.Runtime — Design Specification

The `CTI.AIDev.Runtime` subsystem provides the foundational execution layer for all AI model operations. It is split into two projects:

- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Runtime`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Runtime.Abstractions

This project defines all runtime‑level interfaces and contracts. It contains no implementations and no references to Microsoft AI libraries.

### CTI.AIDev.Runtime

This project contains the concrete implementation of the runtime, wrapping Microsoft.AI.Foundry.Local and related libraries. It depends on `CTI.AIDev.Runtime.Abstractions` and on the Microsoft AI components.

---

## Purpose

The runtime provides a stable, async‑first abstraction over the local AI execution environment. It manages model instances, device execution, and runtime diagnostics, while hiding all implementation details and external library types behind interfaces.

---

## Responsibilities

The runtime is responsible for:

- Managing model instance lifecycle (load, unload, query).
- Executing inference requests asynchronously.
- Abstracting hardware devices and execution capabilities.
- Routing requests to appropriate execution providers.
- Exposing runtime metrics and diagnostics.
- Providing a single, stable entry point for higher layers.

The runtime implementation uses Microsoft.AI.Foundry.Local and related libraries internally but never exposes them across boundaries.

---

## Non‑responsibilities

The runtime does not:

- Implement agent logic or planning.
- Implement tools or side‑effecting operations.
- Implement profiles, rules, or coding standards.
- Implement memory, retrieval, or embeddings storage.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.

These concerns belong to higher layers.

---

## CTI.AIDev.Runtime.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Runtime.Abstractions`.

### Runtime entry point

#### IRuntimeModelHost

Represents the primary entry point for model execution and lifecycle.

```csharp
public interface IRuntimeModelHost
{
    Task<IModelInstance> LoadModelAsync(
        IModelLoadRequest request,
        CancellationToken cancellationToken = default);

    Task UnloadModelAsync(
        string modelInstanceId,
        CancellationToken cancellationToken = default);

    Task<IInferenceResult> ExecuteAsync(
        string modelInstanceId,
        IInferenceRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IRuntimeMetric>> GetMetricsAsync(
        CancellationToken cancellationToken = default);
}
```

---

### Model instances and kinds

#### IModelInstance

Represents a loaded model instance (base or adapted).

```csharp
public interface IModelInstance
{
    string Id { get; }
    string BaseModelId { get; }
    IModelInstanceKind Kind { get; }
    IDeviceProfile DeviceProfile { get; }
}
```

#### IModelInstanceKind

Represents the discriminator for model instance type.

```csharp
public interface IModelInstanceKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Runtime`):

- `BaseModelInstanceKind : IModelInstanceKind`
- `AdaptedModelInstanceKind : IModelInstanceKind`

These implementations never cross the abstraction boundary.

---

### Model loading

#### IModelLoadRequest

Represents a request to load a model instance.

```csharp
public interface IModelLoadRequest
{
    string ModelId { get; }
    IModelInstanceKind Kind { get; }
    IDevicePreference DevicePreference { get; }
    IRuntimeOptions Options { get; }
}
```

---

### Inference

#### IInferenceRequest

Represents an inference request.

```csharp
public interface IInferenceRequest
{
    string Input { get; }
    float Temperature { get; }
    int MaxTokens { get; }
    string? ProfileId { get; }
}
```

#### IInferenceResult

Represents an inference result.

```csharp
public interface IInferenceResult
{
    string Output { get; }
    IInferenceMetadata Metadata { get; }
}
```

#### IInferenceMetadata

Represents metadata about an inference execution.

```csharp
public interface IInferenceMetadata
{
    string ModelInstanceId { get; }
    string DeviceId { get; }
    long InputTokenCount { get; }
    long OutputTokenCount { get; }
    TimeSpan Latency { get; }
}
```

---

### Devices and execution

#### IDeviceProfile

Represents a hardware execution target.

```csharp
public interface IDeviceProfile
{
    string Id { get; }
    IDeviceKind Kind { get; }
    IDeviceCapabilities Capabilities { get; }
}
```

#### IDeviceKind

Represents the discriminator for device type.

```csharp
public interface IDeviceKind
{
    string Name { get; }
}
```

Concrete implementations (in `CTI.AIDev.Runtime`):

- `CpuDeviceKind : IDeviceKind`
- `GpuDeviceKind : IDeviceKind`
- `NpuDeviceKind : IDeviceKind`

#### IDeviceCapabilities

Represents device capabilities.

```csharp
public interface IDeviceCapabilities
{
    long MaxModelSizeBytes { get; }
    long MaxMemoryBytes { get; }
    double EstimatedThroughput { get; }
}
```

#### IExecutionProvider

Represents an execution provider bound to a device.

```csharp
public interface IExecutionProvider
{
    string Name { get; }
    IDeviceProfile DeviceProfile { get; }

    Task<IInferenceResult> ExecuteAsync(
        IInferenceRequest request,
        CancellationToken cancellationToken = default);
}
```

---

### Configuration and options

#### IRuntimeOptions

Represents runtime configuration.

```csharp
public interface IRuntimeOptions
{
    IReadOnlyList<IDevicePreference> DevicePreferences { get; }
    int MaxConcurrentRequests { get; }
}
```

#### IDevicePreference

Represents a preference for device selection.

```csharp
public interface IDevicePreference
{
    IDeviceKind DeviceKind { get; }
    int Priority { get; }
}
```

---

### Metrics and diagnostics

#### IRuntimeMetric

Represents a single runtime metric.

```csharp
public interface IRuntimeMetric
{
    string Name { get; }
    string Value { get; }
}
```

---

## CTI.AIDev.Runtime — implementation

The `CTI.AIDev.Runtime` project implements all interfaces defined in `CTI.AIDev.Runtime.Abstractions` and integrates with Microsoft AI libraries.

### External dependencies

- `Microsoft.AI.Foundry.Local`
- `Microsoft.AI.Foundry.Local.Core`
- `Microsoft.AI.Foundry.Local.WinML`
- `Microsoft.AI.Foundry.Local.Core.WinML`

These dependencies are used internally to:

- Discover devices and capabilities.
- Register and configure execution providers.
- Load and manage model instances.
- Execute inference requests.
- Collect runtime metrics.

No types from these libraries are exposed through any interface.

---

## Internal architecture (implementation project)

Within `CTI.AIDev.Runtime`, the implementation is organized into:

- **DeviceManager**  
  Discovers hardware, builds `IDeviceProfile` implementations, and registers execution providers.

- **ModelInstanceManager**  
  Manages model instance lifecycle and maps `IModelLoadRequest` to concrete Foundry Local operations.

- **ExecutionRouter**  
  Routes `IInferenceRequest` to the appropriate `IExecutionProvider`.

- **Scheduler**  
  Optionally queues and prioritizes concurrent inference requests.

- **MetricsCollector**  
  Aggregates metrics into `IRuntimeMetric` instances.

All of these are internal classes that implement the abstractions defined in `CTI.AIDev.Runtime.Abstractions`.

---

## Integration with higher layers

Higher layers depend only on `CTI.AIDev.Runtime.Abstractions`:

- `CTI.AIDev.ModelRegistry` uses `IRuntimeModelHost` and `IModelInstance`.
- `CTI.AIDev.Agents` uses `IRuntimeModelHost`, `IInferenceRequest`, and `IInferenceResult`.
- `CTI.AIDev.Tools` may use `IRuntimeModelHost` for AI‑powered tools.
- `CTI.AIDev.Orchestration` uses `IRuntimeModelHost` and `IRuntimeMetric`.

No higher layer references `CTI.AIDev.Runtime` directly unless it is a composition root or host.

---

## Construction and dependency injection

`CTI.AIDev.Runtime` is wired using constructor injection:

- A composition root (e.g., host application) creates concrete implementations.
- Interfaces from `CTI.AIDev.Runtime.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Runtime.Tests` project:

- Tests concrete implementations in `CTI.AIDev.Runtime`.
- Uses fake or local models where possible.
- Verifies correct integration with Foundry Local under controlled conditions.
- Ensures that all interfaces behave as specified.

Higher‑level tests mock `IRuntimeModelHost` and other runtime interfaces without referencing concrete implementations.

---

## Future extensions

The runtime design supports:

- Additional device kinds via new `IDeviceKind` implementations.
- Additional execution providers via new `IExecutionProvider` implementations.
- Advanced scheduling policies without changing abstractions.
- Integration with future Windows AI runtimes by updating only `CTI.AIDev.Runtime`.

The abstractions in `CTI.AIDev.Runtime.Abstractions` remain stable as the system evolves.
