# CTI.AIDev.Profiles — Design Specification

The `CTI.AIDev.Profiles` subsystem defines the behavioral, structural, and domain‑specific constraints that govern how agents and tools operate. It is split into two projects:

- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.Profiles`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Profiles.Abstractions

This project defines all profile‑related interfaces and contracts. It contains no implementations and no references to runtime, model registry, or external libraries.

### CTI.AIDev.Profiles

This project contains the concrete implementation of the profile system. It depends on:

- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.ModelRegistry.Abstractions`

It does not depend on any implementation project.

---

## Purpose

Profiles define the rules, constraints, preferences, and behavioral parameters that guide the system’s reasoning and output. They provide a structured way to encode:

- Architectural rules
- Coding standards
- Naming conventions
- Domain‑specific constraints
- Project‑specific behavior
- Safety and guardrail rules
- Model selection preferences (via Model Registry)

Profiles allow the system to behave differently depending on the project, subsystem, or task context.

---

## Responsibilities

The profile subsystem is responsible for:

- Representing profiles as structured, interface‑based objects.
- Storing and retrieving profile definitions.
- Providing rule sets and constraints to agents and tools.
- Binding profiles to model instances via the Model Registry.
- Supporting hierarchical or layered profiles (solution → project → subsystem).
- Providing profile metadata to orchestration for task routing.
- Ensuring all profile content is interface‑based and testable.

---

## Non‑responsibilities

The profile subsystem does not:

- Execute inference or interact with devices.
- Implement memory, retrieval, or embeddings.
- Implement agent logic or planning.
- Implement tools or side‑effecting operations.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* or runtime implementations.

These concerns belong to other layers.

---

## CTI.AIDev.Profiles.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Profiles.Abstractions`.

### Profile entry point

#### IProfileService

Provides access to profiles and profile resolution.

```csharp
public interface IProfileService
{
    Task<IProfile?> GetProfileByIdAsync(
        string profileId,
        CancellationToken cancellationToken = default);

    Task<IProfile?> ResolveProfileForProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IProfile>> GetAllProfilesAsync(
        CancellationToken cancellationToken = default);
}
```

---

### Profile definition

#### IProfile

Represents a complete profile definition.

```csharp
public interface IProfile
{
    string Id { get; }
    string Name { get; }
    IProfileKind Kind { get; }
    IReadOnlyList<IProfileRule> Rules { get; }
    IReadOnlyList<IProfilePreference> Preferences { get; }
    IReadOnlyList<IProfileBinding> Bindings { get; }
}
```

#### IProfileKind

Represents the discriminator for profile type.

```csharp
public interface IProfileKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Profiles`):

- `SolutionProfileKind : IProfileKind`
- `ProjectProfileKind : IProfileKind`
- `SubsystemProfileKind : IProfileKind`

---

### Profile rules

#### IProfileRule

Represents a single rule within a profile.

```csharp
public interface IProfileRule
{
    string RuleId { get; }
    string Description { get; }
    IRuleCategory Category { get; }
}
```

#### IRuleCategory

Represents the discriminator for rule category.

```csharp
public interface IRuleCategory
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Profiles`):

- `ArchitectureRuleCategory : IRuleCategory`
- `CodingStandardRuleCategory : IRuleCategory`
- `NamingConventionRuleCategory : IRuleCategory`

---

### Profile preferences

#### IProfilePreference

Represents a preference that influences agent or tool behavior.

```csharp
public interface IProfilePreference
{
    string PreferenceId { get; }
    string Description { get; }
    string Value { get; }
}
```

Preferences may include:

- Temperature limits  
- Output formatting  
- Naming patterns  
- Code style preferences  
- Safety constraints  

All represented as interfaces.

---

### Profile bindings

#### IProfileBinding

Represents a binding between a profile and a model or subsystem.

```csharp
public interface IProfileBinding
{
    string BindingId { get; }
    string TargetId { get; }
    IProfileBindingKind Kind { get; }
}
```

#### IProfileBindingKind

Represents the discriminator for binding type.

```csharp
public interface IProfileBindingKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Profiles`):

- `ModelBindingKind : IProfileBindingKind`
- `SubsystemBindingKind : IProfileBindingKind`

---

## CTI.AIDev.Profiles — implementation

The `CTI.AIDev.Profiles` project implements all interfaces defined in `CTI.AIDev.Profiles.Abstractions`.

### Internal responsibilities

- Storing profile definitions (file‑based, embedded, or future DB).
- Implementing hierarchical profile resolution.
- Implementing rule and preference merging.
- Integrating with `IModelRegistry` to bind profiles to model instances.
- Providing profile metadata to agents and orchestration.

### Internal dependencies

- `CTI.AIDev.Profiles.Abstractions`
- `CTI.AIDev.ModelRegistry.Abstractions`

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.Profiles.Abstractions`:

- **Agents** use profiles to constrain reasoning and tool usage.
- **Tools** use profiles to enforce coding standards and rules.
- **Memory** uses profiles to determine what to store or retrieve.
- **Orchestration** uses profiles to select agents and workflows.
- **UI** may display profile information but never depends on implementation.

No higher layer references `CTI.AIDev.Profiles` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.Profiles` is wired using constructor injection:

- A composition root creates concrete implementations of `IProfileService` and related interfaces.
- Interfaces from `CTI.AIDev.Profiles.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Profiles.Tests` project:

- Tests concrete implementations in `CTI.AIDev.Profiles`.
- Uses in‑memory stores for profile definitions.
- Uses mocked `IModelRegistry` from `CTI.AIDev.ModelRegistry.Abstractions`.
- Verifies correct rule merging and profile resolution.
- Verifies correct binding behavior.

Higher‑level tests mock `IProfileService` without referencing concrete implementations.

---

## Future extensions

The profile subsystem supports:

- Additional profile kinds via new `IProfileKind` implementations.
- Additional rule categories via new `IRuleCategory` implementations.
- Additional binding kinds via new `IProfileBindingKind` implementations.
- Dynamic profile generation based on project metadata.
- Integration with future IDE or Windows‑level profile systems.

The abstractions in `CTI.AIDev.Profiles.Abstractions` remain stable as the system evolves.
