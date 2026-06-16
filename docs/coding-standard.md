# C# Coding Standard

**Purpose:**  
This document defines the mandatory C# coding conventions for all CTI.AIDev source code.  
It ensures architectural purity, consistency, readability, and long‑term maintainability across all subsystems.

---

## 1. Naming conventions

### 1.1 Interfaces

- All interfaces **must** be prefixed with `I`.
- Interface names must be descriptive and represent capabilities or contracts.
- Examples:  
  - `IModelDescriptor`  
  - `IRuntimeModelHost`  
  - `IAgentPlanner`

### 1.2 Classes

- Class names must be descriptive and avoid suffixes such as `Service`, `Manager`, or `Helper`.
- Prefer domain‑meaningful names:  
  - `ModelRegistry` instead of `ModelRegistryService`  
  - `RuntimeModelHost` instead of `RuntimeModelHostManager`

### 1.3 Methods

- All asynchronous methods **must** end with `Async`.
- Method names must be verbs or verb phrases describing the action.

### 1.4 Properties

- Properties use **PascalCase**.
- Properties must be descriptive and avoid abbreviations.

### 1.5 Variables

- Variables use **camelCase**.
- No leading underscores (`_value` is forbidden).
- Avoid abbreviations unless universally understood (`id`, `uri`, `cpu`).

### 1.6 Namespaces

- Namespaces follow project and folder structure exactly.
- Format:  
  - `CTI.AIDev.Runtime.Abstractions`  
  - `CTI.AIDev.ModelRegistry.Internal`  
- No “utility” or “common” namespaces.

---

## 2. File and folder structure

### 2.1 One type per file

- Every file contains exactly **one** type.
- File name must match the type name.

### 2.2 Folder usage

- Folders represent **internal boundaries only**.
- `.Abstractions` projects contain only interfaces; no folder segregation required.
- Implementation projects may use folders to group internal components.

### 2.3 Partial classes

- Avoid partial classes unless required by .NET (e.g., source generators, WinForms).

### 2.4 Records

- Records are avoided unless representing **pure DTOs** with no behavior.
- DTOs must still follow interface‑only boundary rules.

---

## 3. Patterns and dependency management

### 3.1 Dependency Injection

- Constructor injection is the **only** allowed dependency injection mechanism.
- No property injection.
- No method injection.

### 3.2 Service Locator

- The Service Locator pattern is **strictly forbidden**.

### 3.3 Factories and Builders

- Allowed only when:
  - Dependency injection cannot be used, **and**
  - The factory does not act as a service locator.

### 3.4 Extension methods

- Allowed and encouraged as **static decorators**.
- Must not leak concrete types across boundaries.
- Each project must expose an `IServiceCollection` extension for DI registration.

### 3.5 Static classes

- Static classes are **not allowed**, except:
  - Extension method containers (which must be `static` by C# requirement).

### 3.6 LINQ usage

- LINQ is allowed only when it improves clarity.
- Avoid complex LINQ chains; prefer explicit loops for readability.

---

## 4. Error handling and robustness

### 4.1 Exception handling

- Never swallow exceptions.
- Always log exceptions with full details.
- Use request/response semantics where possible instead of throwing.

### 4.2 Retry and resilience

- Use Polly or equivalent resilience libraries for:
  - Transient failures  
  - External resource access  
  - Runtime interactions  

### 4.3 Error locality

- Handle errors as close to the source as possible.
- Do not propagate raw exceptions across subsystem boundaries.

### 4.4 Service continuity

- Prefer degraded operation with error reporting over service failure.
- All long‑running services must remain operational even under partial failure.

### 4.5 Logging and telemetry

- All services must accept an injected logger.
- Telemetry must be emitted for:
  - Errors  
  - Performance metrics  
  - External calls  
  - Model execution events  

---

## 5. Comments and documentation

### 5.1 File header

Every file must begin with:

```csharp
/*
 * Name: <fully qualified type name>
 *
 * Purpose: <one-sentence description of the type>
 *
 * © 2026 Concept Tech Inc. All rights reserved.
 */
```

### 5.2 XML documentation

- XML comments are required for all public interfaces.
- XML comments are optional for internal classes unless clarity requires them.

### 5.3 Inline comments

- Inline comments should be rare.
- Only used when the code is not self‑explanatory.

---

## 6. Architectural purity rules

### 6.1 Interface‑only boundaries

- All subsystem boundaries use interfaces exclusively.
- No concrete types appear in any public API.

### 6.2 No enums across boundaries

- All discriminators must be interface‑based (`IModelKind`, `IDeviceKind`, etc.).

## 6.3 No concrete DTOs across boundaries

- All DTOs must be interfaces.
- Implementation classes remain internal.

### 6.4 No default interface implementations

- Interfaces must not contain logic.

### 6.5 No static global state

- No singletons except DI‑managed singletons.
- No static caches or global registries.

---

## 7. Asynchronous programming rules

### 7.1 Async everywhere

- All I/O, runtime calls, and long‑running operations must be asynchronous.

### 7.2 Cancellation tokens

- All async methods must accept a `CancellationToken` with a default value.

### 7.3 ConfigureAwait

- Use `ConfigureAwait(false)` only in library code that is not part of the application host.

### 7.4 Timeouts

- All external calls must have explicit timeouts.

---

## 8. Testing standards

### 8.1 Framework

- xUnit is the standard testing framework.

### 8.2 Mocking

- Use NSubstitute consistently across all tests.

### 8.3 Naming

- Test classes mirror the type under test.
- Test methods follow the pattern:  
  - `MethodName_ShouldDoSomething_WhenCondition`

### 8.4 Structure

- One test class per type.
- One assertion per test unless testing a sequence.

### 8.5 Test data

- Use builders or inline data; avoid magic values.

---

## 9. Logging and telemetry

### 9.1 Logging

- Use `ILogger<T>` injected via constructor.
- Log at appropriate levels:
  - Debug: internal flow  
  - Information: major events  
  - Warning: recoverable issues  
  - Error: failures  
  - Critical: system‑level failures  

### 9.2 Telemetry

- Emit structured telemetry for:
  - Model execution  
  - Tool execution  
  - Memory operations  
  - Agent steps  
  - Orchestration flows  

---

## 10. Code style

- Use explicit types (`var` only when the type is obvious).
- Use expression‑bodied members sparingly.
- Avoid deeply nested logic.
- Prefer early returns to reduce indentation.
- Avoid magic numbers and strings.
- Keep methods short and focused.

---

## 11. File endings and formatting

- UTF‑8 encoding.
- Unix line endings (`\n`).
- No trailing whitespace.
- Final newline at end of file.
- Use standard .editorconfig to enforce formatting.

---

## 12. Compliance

All code must comply with this standard.  
Non‑compliant code must not be merged.  
Automated analyzers and CI checks must enforce these rules.
