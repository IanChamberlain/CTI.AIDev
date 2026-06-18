# CTI.AIDev.AppHost — Design Specification

The `CTI.AIDev.AppHost` project is the **composition root** of the entire system.  
It is the only project responsible for:

- Creating and owning the DI container  
- Loading configuration  
- Configuring logging and telemetry  
- Instantiating the `ServiceConfigurator`  
- Registering all subsystem services  
- Building the final `IServiceProvider`  
- Starting the application’s UI or host loop  

All other projects register their own services but do **not** create or own DI.

---

## Project structure

### CTI.AIDev.AppHost

This project is the **outermost executable**.  
It contains:

- `Program.cs` (entry point)  
- `CompositionRoot.cs` (optional organizational class)  
- `ServiceConfigurator.cs` (DI wrapper)  
- No business logic  
- No subsystem logic  
- No concrete types from other subsystems except through DI registration  

It depends on:

- `Microsoft.Extensions.DependencyInjection`  
- `Microsoft.Extensions.Logging`  
- `Microsoft.Extensions.Configuration`  
- All subsystem **implementation** projects  
- All subsystem **abstractions** projects  

It does **not** depend on any UI framework unless the UI subsystem requires it.

---

## Purpose

The AppHost project provides:

- A single, authoritative place to configure the application  
- A deterministic DI container  
- A consistent logging and telemetry pipeline  
- A unified configuration model  
- A startup sequence that wires all subsystems together  

It is the only project allowed to know about:

- Concrete types from all subsystems  
- Logging providers  
- Configuration sources  
- Host environment details  

---

## Responsibilities

The AppHost project is responsible for:

- Creating the `IServiceCollection`  
- Creating the `ServiceConfigurator`  
- Loading configuration from:
  - `appsettings.json`  
  - `appsettings.<Environment>.json`  
  - Environment variables  
  - Command‑line arguments  
- Configuring logging from configuration  
- Calling each subsystem’s top‑level registration method  
- Building the final `IServiceProvider`  
- Starting the UI subsystem  
- Managing application lifetime  

---

## Non‑responsibilities

The AppHost project does **not**:

- Implement business logic  
- Implement agents, tools, memory, profiles, or runtime  
- Implement UI logic beyond starting it  
- Contain any domain logic  
- Contain any concrete service implementations except `ServiceConfigurator`  

It is strictly a composition and startup project.

---

## Service Configurator

### Service Configurator Purpose

`ServiceConfigurator` wraps the DI container and provides a controlled, architecture‑aligned registration API.

### Service Configurator Responsibilities

- Hold the `IServiceCollection`  
- Hold the `IConfiguration`  
- Provide `AddService<TService, TImplementation>(Lifetime lifetime)`  
- Provide `AddService<TService>(factory, lifetime)`  
- Internally use `TryAdd` for all registrations  
- Provide access to configuration for all subsystems  
- Provide access to logging configuration  

### Service Configurator Characteristics

- Concrete class  
- Created only in AppHost  
- Passed to all subsystem registration methods  
- Never used as a service locator  
- Never exposed outside composition  

---

## Composition flow

### 1. Build configuration

- Load base configuration  
- Load environment‑specific configuration  
- Load environment variables  
- Load command‑line arguments  

### 2. Configure logging

- Use `ILoggerFactory` and `ILogger<T>`  
- Configure from configuration  
- Register logging providers (Console, Debug, EventSource, etc.)  

### 3. Create DI container

```csharp
var services = new ServiceCollection();
var configurator = new ServiceConfigurator(services, configuration);
```

### 4. Register subsystems

In strict dependency order:

```csharp
configurator.AddRuntime();
configurator.AddModelRegistry();
configurator.AddProfiles();
configurator.AddMemory();
configurator.AddTools();
configurator.AddAgents();
configurator.AddOrchestration();
configurator.AddUI();
```

Each subsystem:

- Registers its own services  
- Registers its own dependencies
- Chooses its own lifetimes  

### 5. Build provider

```csharp
var provider = configurator.Build();
```

### 6. Start the UI

The UI subsystem exposes an entry point such as:

```csharp
var ui = provider.GetRequiredService<IUiController>();
ui.Run();
```

The AppHost does not implement UI logic.

---

## Integration with subsystems

The AppHost project:

- Calls each subsystem’s top‑level registration method  
- Does not know about internal subsystem structure  
- Does not reference subsystem concrete types directly  
- Does not configure subsystem services manually  

Subsystems are responsible for:

- Registering their own services  
- Registering their own dependencies  
- Providing extension methods for DI  

---

## Logging and telemetry

The AppHost project:

- Configures logging first  
- Ensures all subsystems receive `ILogger<T>`  
- Configures telemetry providers  
- Ensures telemetry is available before any subsystem registers  

Subsystems:

- Must not configure logging  
- Must not configure telemetry  
- Must only consume injected loggers  

---

## Testing considerations

- Integration tests may create a minimal AppHost instance  
- Tests may override services using `AddService<TService>(factory, lifetime)`  
- Because `TryAdd` is used, test overrides always win  
- No subsystem may create its own DI container  

---

## Future extensions

The AppHost project supports:

- Multiple UI front‑ends (CLI, desktop, IDE plugin)  
- Multiple runtime configurations  
- Environment‑specific composition  
- Feature‑flagged subsystem registration  
- Plugin‑based subsystem discovery (future)  

The AppHost remains the single, authoritative composition root.
