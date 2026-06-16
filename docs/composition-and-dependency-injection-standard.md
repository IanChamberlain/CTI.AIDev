# Composition and Dependency Injection Specification

**Purpose:**  
This document defines the dependency injection and composition rules for all CTI.AIDev subsystems.  
It ensures deterministic service registration, subsystem isolation, and consistent construction across the entire solution.

---

## 1. DI container

The system uses **Microsoft.Extensions.DependencyInjection** as the dependency injection container.

- All service registration flows through a central `ServiceConfigurator`.
- No subsystem may reference or instantiate `ServiceCollection` directly.
- No subsystem may use or implement a service locator.

---

## 2. ServiceConfigurator

### 2.1 Purpose

`ServiceConfigurator` is the **single composition abstraction** used across the entire solution.  
It wraps the underlying `IServiceCollection` and exposes controlled registration methods.

### 2.2 Responsibilities

- Hold the `IServiceCollection` instance.
- Provide controlled service registration via `AddService<TService, TImplementation>(Lifetime lifetime)`.
- Internally use `TryAdd` to prevent duplicate registrations.
- Provide access to configuration (`IConfiguration`).
- Provide access to logging configuration.
- Provide access to any environment or host‑level services required during composition.

### 2.3 Definition

`ServiceConfigurator` must:

- Be a concrete class.
- Be constructed in the composition root.
- Expose:
  - `IServiceCollection Services { get; }`
  - `IConfiguration Configuration { get; }`
  - `AddService<TService, TImplementation>(Lifetime lifetime)`  
  - `AddService<TService>(Func<IServiceProvider, TService> factory, Lifetime lifetime)`  
- Use `TryAdd` internally for all registrations.
- Never expose the underlying `ServiceCollection` directly except through the `Services` property.

---

## 3. Service lifetimes

Each service chooses its own lifetime via its **own extension method**.

Allowed lifetimes:

- Singleton  
- Scoped  
- Transient  

Rules:

- Runtime components are typically **Singleton**.
- Agents are typically **Transient**.
- Tools may be **Singleton** or **Transient** depending on statefulness.
- Memory and Model Registry are typically **Singleton**.
- UI controllers are typically **Transient**.

---

## 4. Project‑level registration

Each project must provide:

1. **A top‑level extension method**  
   - Named `Add<ProjectName>(this ServiceConfigurator configurator)`
   - Located in the implementation project
   - Called from the composition root

2. **Internal extension methods** for each service  
   - Named `Add<ServiceName>(this ServiceConfigurator configurator)`
   - Responsible for:
     - Registering dependencies first
     - Registering the service itself
     - Choosing the lifetime
     - Using `TryAdd` via `AddService<TService, TImplementation>`

3. **No project may register services belonging to another project.**

Example structure:

```csharp
CTI.AIDev.Runtime
  RuntimeModelHost.cs
  RuntimeModelHostExtensions.cs
  RuntimeCompositionExtensions.cs
```

---

## 5. Service registration rules

### 5.1 Each service registers itself

Every service must provide an extension method:

```csharp
public static ServiceConfigurator AddXyz(this ServiceConfigurator configurator)
```

This method:

- Registers all dependencies first.
- Registers the service itself.
- Chooses the lifetime.
- Uses `AddService<TService, TImplementation>`.

### 5.2 Dependencies must be registered before the service

Example:

```csharp
configurator.AddDeviceManager();
configurator.AddExecutionProvider();
configurator.AddRuntimeModelHost();
```

### 5.3 TryAdd semantics

All registrations must use `TryAdd` internally to avoid duplicates.

This allows:

- Multiple projects calling the same registration safely.
- Services to be overridden in tests or host environments.

---

## 6. Logging and configuration

### 6.1 Logging is configured first

The composition root must:

1. Build configuration  
2. Configure logging from configuration  
3. Create the `ServiceConfigurator`  
4. Register all subsystems  

Logging must be available before any subsystem registers services.

### 6.2 Configuration access

`ServiceConfigurator` exposes:

- `IConfiguration Configuration { get; }`

All subsystems must read configuration **only** through the configurator.

---

## 7. Composition root

The composition root:

- Lives in the host application (e.g., CLI, UI, or service host).
- Creates the `ServiceCollection`.
- Builds configuration.
- Configures logging.
- Creates the `ServiceConfigurator`.
- Calls each subsystem’s top‑level registration method:
  - `configurator.AddRuntime()`
  - `configurator.AddModelRegistry()`
  - `configurator.AddProfiles()`
  - `configurator.AddMemory()`
  - `configurator.AddTools()`
  - `configurator.AddAgents()`
  - `configurator.AddOrchestration()`
  - `configurator.AddUI()`

The composition root must not contain business logic.

---

## 8. Extension method structure

### 8.1 Top‑level project extension

```csharp
public static class RuntimeCompositionExtensions
{
    public static ServiceConfigurator AddRuntime(this ServiceConfigurator configurator)
    {
        configurator.AddDeviceManager();
        configurator.AddExecutionProviders();
        configurator.AddRuntimeModelHost();
        return configurator;
    }
}
```

### 8.2 Service‑level extension

```csharp
public static class RuntimeModelHostExtensions
{
    public static ServiceConfigurator AddRuntimeModelHost(this ServiceConfigurator configurator)
    {
        configurator.AddDeviceManager();
        configurator.AddExecutionProviders();

        configurator.AddService<IRuntimeModelHost, RuntimeModelHost>(Lifetime.Singleton);

        return configurator;
    }
}
```

---

## 9. Lifetime selection

Each service chooses its own lifetime based on:

- Statefulness  
- Thread safety  
- Performance  
- Expected usage frequency  

The service’s extension method is the **only** place where lifetime is chosen.

---

## 10. Telemetry and logging integration

All services must:

- Accept `ILogger<T>` via constructor injection.
- Emit structured logs.
- Emit telemetry events for:
  - Model execution
  - Tool execution
  - Memory operations
  - Agent steps
  - Orchestration flows

Telemetry providers are registered in the composition root.

---

## 11. Testing and overrides

- Tests may override services by calling `AddService<TService>(factory, lifetime)`.
- Because `TryAdd` is used internally, test overrides always win.
- No service may be sealed against replacement.

---

## 12. Compliance

All subsystems must follow this composition model.  
Non‑compliant registration patterns must not be merged.  
Automated analyzers and CI checks must enforce these rules.
