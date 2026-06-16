# CTI.AIDev.ModelRegistry — Design Specification

The `CTI.AIDev.ModelRegistry` subsystem defines the logical model catalog and project‑specific model bindings for the system. It is split into two projects:

- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.ModelRegistry`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.ModelRegistry.Abstractions

This project defines all model registry interfaces and contracts. It contains no implementations and no references to runtime or external libraries.

### CTI.AIDev.ModelRegistry

This project contains the concrete implementation of the model registry. It depends on:

- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`

It does not depend on `CTI.AIDev.Runtime` directly except in composition roots.

---

## Purpose

The model registry provides a structured, project‑aware catalog of all models known to the system. It tracks base models, adapted models, and project‑specific bindings, and coordinates with the runtime to ensure that the correct model instances are available when needed.

---

## Responsibilities

The model registry is responsible for:

- Maintaining metadata for all models known to the system.
- Tracking relationships between base models and adapted variants.
- Managing project‑specific model bindings and preferences.
- Coordinating with the runtime to load and unload model instances.
- Providing lookup APIs to resolve model instances for a given project and purpose.
- Representing adaptation artifacts and their lineage as interfaces.

The registry does not execute models; it only describes and resolves them.

---

## Non‑responsibilities

The model registry does not:

- Execute inference or interact with devices.
- Implement profiles, rules, or coding standards.
- Implement memory, retrieval, or embeddings storage.
- Implement agent logic or planning.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* directly.

These concerns belong to other layers.

---

## CTI.AIDev.ModelRegistry.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.ModelRegistry.Abstractions`.

### Registry entry point

#### IModelRegistry

Represents the primary entry point for model metadata and resolution.

```csharp
public interface IModelRegistry
{
    Task<IModelDescriptor?> GetModelByIdAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IModelDescriptor>> GetAllModelsAsync(
        CancellationToken cancellationToken = default);

    Task<IProjectModelBinding?> GetProjectBindingAsync(
        string projectId,
        string purposeId,
        CancellationToken cancellationToken = default);

    Task<IModelInstanceResolution> ResolveModelInstanceAsync(
        IModelResolutionRequest request,
        CancellationToken cancellationToken = default);

    Task RegisterModelAsync(
        IModelDescriptor descriptor,
        CancellationToken cancellationToken = default);

    Task RegisterProjectBindingAsync(
        IProjectModelBinding binding,
        CancellationToken cancellationToken = default);
}
```

---

### Models and kinds

#### IModelDescriptor

Represents a logical model known to the system.

```csharp
public interface IModelDescriptor
{
    string Id { get; }
    string Name { get; }
    IModelKind Kind { get; }
    string ProviderId { get; }
}
```

#### IModelKind

Represents the discriminator for model type.

```csharp
public interface IModelKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.ModelRegistry`):

- `BaseTextModelKind : IModelKind`
- `EmbeddingModelKind : IModelKind`
- `AudioModelKind : IModelKind`

These implementations never cross the abstraction boundary.

---

### Adaptation and lineage

#### IAdaptedModelDescriptor

Represents an adapted model derived from a base model.

```csharp
public interface IAdaptedModelDescriptor : IModelDescriptor
{
    string BaseModelId { get; }
    IAdaptationKind AdaptationKind { get; }
    IAdaptationArtifactReference ArtifactReference { get; }
}
```

#### IAdaptationKind

Represents the discriminator for adaptation type.

```csharp
public interface IAdaptationKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.ModelRegistry`):

- `LoraAdaptationKind : IAdaptationKind`
- `FineTunedAdaptationKind : IAdaptationKind`

#### IAdaptationArtifactReference

Represents a reference to adaptation artifacts.

```csharp
public interface IAdaptationArtifactReference
{
    string Location { get; }
    string Format { get; }
}
```

---

### Project bindings

#### IProjectModelBinding

Represents a binding between a project, a purpose, and a model.

```csharp
public interface IProjectModelBinding
{
    string ProjectId { get; }
    string PurposeId { get; }
    string ModelId { get; }
}
```

`PurposeId` is a semantic identifier such as `"coding"`, `"analysis"`, or `"embeddings"`.

---

### Resolution

#### IModelResolutionRequest

Represents a request to resolve a model instance for a project and purpose.

```csharp
public interface IModelResolutionRequest
{
    string ProjectId { get; }
    string PurposeId { get; }
}
```

#### IModelInstanceResolution

Represents the result of resolving a model instance.

```csharp
public interface IModelInstanceResolution
{
    IModelDescriptor ModelDescriptor { get; }
    string ModelInstanceId { get; }
}
```

The `ModelInstanceId` corresponds to an instance managed by `IRuntimeModelHost` from `CTI.AIDev.Runtime.Abstractions`.

---

## CTI.AIDev.ModelRegistry — implementation

The `CTI.AIDev.ModelRegistry` project implements all interfaces defined in `CTI.AIDev.ModelRegistry.Abstractions` and coordinates with the runtime.

### Internal responsibilities

- Persisting model descriptors and project bindings.
- Managing adapted model descriptors and their lineage.
- Implementing resolution logic for `IModelResolutionRequest`.
- Calling `IRuntimeModelHost` to ensure required model instances are loaded.
- Caching resolution results where appropriate.

### Internal dependencies

- `CTI.AIDev.ModelRegistry.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`

The implementation never exposes runtime types directly; it only uses runtime interfaces internally.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.ModelRegistry.Abstractions`:

- **Profiles** use `IModelRegistry` to bind profiles to specific models.
- **Memory** uses `IModelRegistry` to determine which models to use for embeddings or reasoning.
- **Agents** use `IModelRegistry` to resolve model instances for specific tasks.
- **Orchestration** uses `IModelRegistry` to select models for workflows.

No higher layer references `CTI.AIDev.ModelRegistry` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.ModelRegistry` is wired using constructor injection:

- A composition root creates concrete implementations of `IModelRegistry` and related interfaces.
- Interfaces from `CTI.AIDev.ModelRegistry.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.ModelRegistry.Tests` project:

- Tests concrete implementations in `CTI.AIDev.ModelRegistry`.
- Uses in‑memory stores or fakes for persistence.
- Uses mocked `IRuntimeModelHost` from `CTI.AIDev.Runtime.Abstractions`.
- Verifies correct resolution behavior for various project and purpose combinations.
- Verifies correct handling of adapted models and lineage.

Higher‑level tests mock `IModelRegistry` without referencing concrete implementations.

---

## Future extensions

The model registry design supports:

- Additional model kinds via new `IModelKind` implementations.
- Additional adaptation kinds via new `IAdaptationKind` implementations.
- Multiple providers and backends via `ProviderId`.
- Model deprecation and migration strategies implemented internally.
- Integration with future Windows AI model catalogs by updating only `CTI.AIDev.ModelRegistry`.

The abstractions in `CTI.AIDev.ModelRegistry.Abstractions` remain stable as the system evolves.

