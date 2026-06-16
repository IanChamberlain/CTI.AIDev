# System architecture

This document describes the overall architecture of the local AI‑accelerated software building system. The system is a local, hardware‑aware, multi‑model environment that supports agentic workflows, per‑project AI profiles, and user‑controlled teaching and learning. Models internalize knowledge about the systems they work on, enabling them to operate with the same familiarity and intuition as a human developer.

All subsystem boundaries are defined by interfaces in `.Abstractions` projects. All complex types, including DTOs, are interfaces. No enums or concrete types cross subsystem boundaries. Microsoft AI runtime components are confined to a single implementation project.

---

## Architectural overview

The system is organized into the following core subsystems:

- **Runtime** (`CTI.AIDev.Runtime`)  
- **Model Registry** (`CTI.AIDev.ModelRegistry`)  
- **Profile System** (`CTI.AIDev.Profiles`)  
- **Memory and Retrieval** (`CTI.AIDev.Memory`)  
- **Tooling Layer** (`CTI.AIDev.Tools`)  
- **Agent Framework** (`CTI.AIDev.Agents`)  
- **Orchestration Layer** (`CTI.AIDev.Orchestration`)  
- **UI Layer** (`CTI.AIDev.UI`)  
- **Teaching and Learning** (cross‑cutting behavior across Profiles, Memory, Model Registry, Runtime)

Each subsystem has a clear responsibility and interacts with others only through interfaces defined in its `.Abstractions` project. Implementation projects depend on abstractions; abstractions never depend on implementations.

---

## Runtime

The Runtime is the hardware‑abstracted execution layer for AI models.

### Runtime responsibilities

- Load and unload model instances.
- Execute inference requests asynchronously.
- Abstract NPU, GPU, and CPU as device profiles and capabilities.
- Expose a consistent, interface‑only API for model execution.
- Wrap Microsoft local AI runtime components without exposing them.

### Runtime characteristics

- Uses Microsoft.AI.Foundry.Local and related components **only** inside `CTI.AIDev.Runtime` (implementation).
- Represents devices, model instances, and execution providers via interfaces (`IDeviceProfile`, `IModelInstance`, `IExecutionProvider`).
- Supports multiple concurrent model instances and device‑aware scheduling.
- Supports model adaptation workflows via model instance kinds and options, without leaking implementation details.

---

## Model registry

The Model Registry is the catalog of all logical models known to the system.

### Model registry responsibilities

- Store metadata for each model as interfaces (`IModelDescriptor`, `IAdaptedModelDescriptor`).
- Track relationships between base models and adapted variants.
- Provide lookup and selection APIs for Orchestration, Agents, and Profiles.
- Manage project‑specific model bindings (`IProjectModelBinding`) that internalize repository knowledge.
- Coordinate with the Runtime to ensure required model instances are available.

### Model registry characteristics

- Supports small, specialized models and general reasoning models.
- Allows multiple instances of the same base model with different learned knowledge.
- Maintains lineage and provenance for all adapted models via interface‑based adaptation kinds and artifact references.
- Depends only on `CTI.AIDev.Runtime.Abstractions`; it never references runtime implementations or Microsoft components directly.

---

## Profile system

The Profile System defines how the AI behaves in different contexts, especially per project or per subsystem.

### Profile system responsibilities

- Represent project‑specific rules, constraints, and preferences as interfaces (`IProfile`, `IProfileRule`, `IProfilePreference`).
- Capture architecture guidelines, coding standards, and domain language.
- Bind models, tools, and memory to a specific context via profile bindings (`IProfileBinding`).
- Provide profile resolution services (`IProfileService`) for Orchestration and Agents.

### Profile system characteristics

- Profiles are explicit, inspectable, and versionable.
- Profiles guide both inference and learning by constraining agents, tools, and memory usage.
- Profiles define what knowledge should be internalized by models and what remains external in memory.
- Depends only on `CTI.AIDev.ModelRegistry.Abstractions` for model‑related bindings.

---

## Memory and retrieval

The Memory and Retrieval subsystem provides long‑term, structured knowledge storage and semantic retrieval.

### Memory and retrieval responsibilities

- Store and retrieve memory entries (`IMemoryEntry`) for repositories, documents, decisions, and patterns.
- Maintain project‑scoped and global memory, including architectural decisions and recurring patterns.
- Provide retrieval interfaces (`IMemoryService`, `IMemoryQuery`) for Agents and Orchestration.
- Generate embeddings via the Runtime (`IEmbeddingRequest`, `IEmbeddingVector`) for semantic search.
- Support model adaptation by supplying curated training material.

### Memory and retrieval characteristics

- Uses retrieval‑augmented generation for dynamic context.
- Supports incremental updates as repositories and documentation change.
- Separates transient session context from persistent project memory.
- Enforces profile‑based rules for what may be stored or retrieved.
- Depends only on `CTI.AIDev.Runtime.Abstractions` and `CTI.AIDev.Profiles.Abstractions`.

---

## Tooling layer

The Tooling Layer exposes capabilities that agents can invoke to interact with the environment.

### Tooling layer responsibilities

- Provide file and repository operations as tools (`ITool`).
- Run builds, tests, and analysis tools.
- Execute system commands in a controlled manner.
- Offer domain‑specific tools where needed (e.g., telemetry, diagnostics).
- Enforce profile‑based constraints on tool usage (`IToolConstraint`).

### Tooling layer characteristics

- Tools are explicit, typed operations with interface‑based requests and results (`IToolRequest`, `IToolResult`).
- All tool usage is logged and reviewable at the orchestration/agent level.
- Tools are the only way agents modify code or external state.
- Depends only on `CTI.AIDev.Runtime.Abstractions`, `CTI.AIDev.Profiles.Abstractions`, and optionally `CTI.AIDev.Memory.Abstractions`.

---

## Agent framework

The Agent Framework provides the structure for AI components that can reason, plan, and act.

### Agent framework responsibilities

- Implement agent loops that plan, call tools, observe results, and refine actions (`IAgent`, `IAgentPlanner`, `IAgentReasoner`).
- Enforce profile constraints during decision‑making by consuming `IProfile`.
- Coordinate with Orchestration for task context and model selection.
- Use Memory for context retrieval and result storage.
- Use Tools for all side‑effectful operations.

### Agent framework characteristics

- Agents are stateless between invocations; state lives in Memory, Profiles, and model‑internalized knowledge.
- Agents operate through explicit, interface‑based steps (`IAgentStepResult`) that are observable and auditable.
- Different agent kinds exist for tasks such as coding, analysis, validation, and explanation (`IAgentKind`).
- Depends only on abstractions: Runtime, Tools, Memory, Profiles.

---

## Orchestration layer

The Orchestration Layer coordinates models, agents, profiles, tools, and memory into coherent workflows.

### Orchestration layer responsibilities

- Accept high‑level tasks from the UI (`IOrchestrationTask`).
- Resolve the relevant profile, model instance, and agent(s).
- Plan multi‑step workflows (`IOrchestrationPlanner`, `IOrchestrationPlanStep`).
- Manage execution flow and progress for long‑running operations.
- Aggregate agent results into structured orchestration results (`IOrchestrationResult`).
- Enforce system‑wide policies and safety constraints.

### Orchestration layer characteristics

- Acts as the central decision point for which components participate in a given task.
- Supports composition of multiple agents and models for complex workflows.
- Provides logging and tracing for all orchestrated operations.
- Selects between retrieval‑based reasoning and model‑internalized knowledge depending on the task.
- Depends only on abstractions: Agents, Tools, Memory, Profiles, Model Registry, Runtime.

---

## UI layer

The UI Layer provides the presentation and interaction surface for the user.

### UI layer responsibilities

- Accept user input and convert it into orchestration tasks (`IUiTaskRequest` → `IOrchestrationTask`).
- Display orchestration results in a structured, user‑friendly format (`IUiRenderedResult`).
- Display progress and status updates for long‑running tasks (`IUiStatusResponse`).
- Provide a consistent visual and interaction model across the system.

### UI layer characteristics

- Depends only on `CTI.AIDev.Orchestration.Abstractions`.
- Never interacts directly with Agents, Tools, Memory, Runtime, or Model Registry.
- Uses interface‑based DTOs for all UI‑level data (`IUiTaskRequest`, `IUiTaskResponse`, `IUiRenderedResult`).
- Serves as the outermost boundary; all internal details remain hidden behind abstractions.

---

## Teaching and learning

Teaching and Learning is a cross‑cutting concern implemented through Profiles, Memory, Model Registry, and Runtime.

### Teaching and learning responsibilities

- Capture user feedback on AI outputs (approval, rejection, corrections).
- Convert feedback into updates to profiles, rules, and memory.
- Manage model adaptation workflows that internalize project knowledge via the Model Registry and Runtime.
- Ensure models learn the systems they work on in a controlled, explicit way.

### Teaching and learning characteristics

- Uses simple, explicit user actions as signals.
- Applies changes through profiles, rules, memory, and model adaptation, all via interfaces.
- Keeps all learning transparent, inspectable, and reversible.
- Supports lightweight adaptation mechanisms (e.g., adapters, fine‑tuning) implemented inside Runtime and Model Registry.
- Reduces reliance on large context windows by embedding knowledge directly into model parameters where appropriate.

---

## Interaction model

A typical interaction follows this flow:

1. The user initiates a task within a project context via the UI (`IUiTaskRequest`).
2. The UI converts the request into an orchestration task and calls `IOrchestrationService`.
3. The Orchestration Layer:
   - Resolves the relevant profile via `IProfileService`.
   - Resolves the appropriate model instance via `IModelRegistry` and `IRuntimeModelHost`.
   - Selects and configures the appropriate agent(s) via `IAgentSelector`.
4. Agents use:
   - Runtime (`IRuntimeModelHost`) for reasoning and embeddings.
   - Memory (`IMemoryService`) for context and persistence.
   - Tools (`ITool`) for all side‑effectful operations.
5. The Orchestration Layer aggregates agent results into an `IOrchestrationResult`.
6. The UI renders the result via `IUiResultPresenter` and presents it to the user.
7. The user reviews, approves, or corrects the output.
8. Teaching and Learning mechanisms update Profiles, Memory, and model‑internalized knowledge through their respective interfaces.

This architecture yields a local, controllable, and extensible AI environment for building software systems, with strict interface‑only boundaries, clean separation of concerns, and Microsoft runtime components confined to a single, well‑defined implementation layer.
