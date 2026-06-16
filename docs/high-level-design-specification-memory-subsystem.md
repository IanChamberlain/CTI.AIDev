# CTI.AIDev.Memory — Design Specification

The `CTI.AIDev.Memory` subsystem provides long‑term memory, retrieval, and structured knowledge storage for the system. It is split into two projects:

- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Memory`

All boundaries are defined by interfaces. All complex types, including DTOs, are interfaces. No enums or concrete types appear in any boundary.

---

## Project structure

### CTI.AIDev.Memory.Abstractions

This project defines all memory‑related interfaces and contracts. It contains no implementations and no references to runtime, profiles, or external libraries.

### CTI.AIDev.Memory

This project contains the concrete implementation of the memory subsystem. It depends on:

- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`

It does not depend on any implementation project.

---

## Purpose

The memory subsystem provides persistent, structured knowledge storage for:

- Project history  
- Architectural decisions  
- Code embeddings  
- Document embeddings  
- Agent reasoning traces  
- User‑provided knowledge  
- Teaching and learning artifacts  

Memory enables agents to operate with continuity, context, and long‑term understanding.

---

## Responsibilities

The memory subsystem is responsible for:

- Storing and retrieving memory entries as interface‑based objects.
- Generating embeddings using runtime models.
- Providing semantic search and retrieval APIs.
- Maintaining project‑scoped and global memory stores.
- Supporting multiple memory types (text, embeddings, structured knowledge).
- Providing memory access to agents, tools, and orchestration.
- Enforcing profile‑based rules for what may be stored or retrieved.

---

## Non‑responsibilities

The memory subsystem does not:

- Execute inference beyond embedding generation.
- Implement agent logic or planning.
- Implement tools or side‑effecting operations.
- Implement orchestration or workflow management.
- Implement any UI or user interaction.
- Reference Microsoft.AI.Foundry.* or runtime implementations directly.

These concerns belong to other layers.

---

## CTI.AIDev.Memory.Abstractions — interfaces

All interfaces below live in `CTI.AIDev.Memory.Abstractions`.

### Memory entry point

#### IMemoryService

Provides access to memory storage and retrieval.

```csharp
public interface IMemoryService
{
    Task StoreAsync(
        IMemoryEntry entry,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IMemoryEntry>> RetrieveAsync(
        IMemoryQuery query,
        CancellationToken cancellationToken = default);

    Task<IEmbeddingVector> GenerateEmbeddingAsync(
        IEmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
```

---

### Memory entries

#### IMemoryEntry

Represents a single memory entry.

```csharp
public interface IMemoryEntry
{
    string EntryId { get; }
    string ProjectId { get; }
    IMemoryKind Kind { get; }
    string Content { get; }
    IEmbeddingVector? Embedding { get; }
    DateTime Timestamp { get; }
}
```

#### IMemoryKind

Represents the discriminator for memory type.

```csharp
public interface IMemoryKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Memory`):

- `TextMemoryKind : IMemoryKind`
- `EmbeddingMemoryKind : IMemoryKind`
- `DecisionMemoryKind : IMemoryKind`
- `CodeMemoryKind : IMemoryKind`

---

### Embeddings

#### IEmbeddingVector

Represents an embedding vector.

```csharp
public interface IEmbeddingVector
{
    IReadOnlyList<float> Values { get; }
}
```

#### IEmbeddingRequest

Represents a request to generate an embedding.

```csharp
public interface IEmbeddingRequest
{
    string Input { get; }
    string ProjectId { get; }
}
```

---

### Memory queries

#### IMemoryQuery

Represents a query for retrieving memory entries.

```csharp
public interface IMemoryQuery
{
    string ProjectId { get; }
    IMemoryQueryKind Kind { get; }
    string? Text { get; }
    IEmbeddingVector? Embedding { get; }
    int MaxResults { get; }
}
```

#### IMemoryQueryKind

Represents the discriminator for query type.

```csharp
public interface IMemoryQueryKind
{
    string Name { get; }
}
```

Examples of concrete implementations (in `CTI.AIDev.Memory`):

- `SemanticQueryKind : IMemoryQueryKind`
- `KeywordQueryKind : IMemoryQueryKind`
- `HybridQueryKind : IMemoryQueryKind`

---

## CTI.AIDev.Memory — implementation

The `CTI.AIDev.Memory` project implements all interfaces defined in `CTI.AIDev.Memory.Abstractions`.

### Internal responsibilities

- Persisting memory entries (file‑based, embedded DB, or future vector DB).
- Generating embeddings using `IRuntimeModelHost`.
- Implementing semantic search using vector similarity.
- Implementing keyword search using text indexing.
- Enforcing profile rules for memory storage and retrieval.
- Supporting project‑scoped and global memory stores.

### Internal dependencies

- `CTI.AIDev.Memory.Abstractions`
- `CTI.AIDev.Runtime.Abstractions`
- `CTI.AIDev.Profiles.Abstractions`

The implementation never exposes concrete types across boundaries.

---

## Integration with other layers

Higher layers depend only on `CTI.AIDev.Memory.Abstractions`:

- **Agents** use memory to retrieve context and store reasoning traces.
- **Tools** use memory to store analysis results or retrieve prior knowledge.
- **Profiles** influence what memory may be stored or retrieved.
- **Orchestration** uses memory to maintain continuity across tasks.
- **UI** may display memory entries but never depends on implementation.

No higher layer references `CTI.AIDev.Memory` directly unless it is a composition root.

---

## Construction and dependency injection

`CTI.AIDev.Memory` is wired using constructor injection:

- A composition root creates concrete implementations of `IMemoryService` and related interfaces.
- Interfaces from `CTI.AIDev.Memory.Abstractions` are registered in the DI container.
- Higher layers receive only interfaces in their constructors.

No service locator, static access, or property injection is used.

---

## Testing strategy

The `CTI.AIDev.Memory.Tests` project:

- Tests concrete implementations in `CTI.AIDev.Memory`.
- Uses in‑memory stores for memory entries.
- Uses mocked `IRuntimeModelHost` for embedding generation.
- Verifies correct semantic and keyword retrieval behavior.
- Verifies correct enforcement of profile rules.

Higher‑level tests mock `IMemoryService` without referencing concrete implementations.

---

## Future extensions

The memory subsystem supports:

- Additional memory kinds via new `IMemoryKind` implementations.
- Additional query kinds via new `IMemoryQueryKind` implementations.
- Advanced vector search backends.
- Memory pruning and summarization.
- Integration with teaching and learning subsystems.
- Multi‑project shared memory spaces.

The abstractions in `CTI.AIDev.Memory.Abstractions` remain stable as the system evolves.
