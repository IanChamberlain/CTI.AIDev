# Source Code

The `/src` directory is the implementation of the local AI‑accelerated software building system. Each component resides in its own project folder and follows naming conventions and structural standards defined in the documentation.

## Purpose

- Implement the hardware‑abstracted model runtime.
- Provide the agent orchestration layer.
- Define the tool‑calling system.
- Manage profiles, memory, and repository‑aware reasoning.
- Support specialized small models and adapters.
- Enable creation of embedded AI capabilities.

## Structure

The structure of the `/src` directory evolves as the system grows. New components are added according to the coding standards and naming conventions defined in `/docs`. Each component resides in its own project folder and aligns with the architectural boundaries of the system.

## Conventions

- Each component has a dedicated project folder.
- Public APIs are documented in code and in `/docs`.
- Components follow loose coupling and clear boundaries.
- New components include:
  - A project folder under `/src`
  - A corresponding test project under `/tests`
  - Documentation updates in `/docs` where applicable
