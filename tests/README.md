# Tests

The `/tests` directory is the automated test suite for the system. Tests ensure correctness, stability, and predictable behavior across the model ecosystem.

## Purpose

- Validate the behavior of the hardware‑abstracted model runtime.
- Test agent planning, tool‑calling, and patch generation.
- Verify profile enforcement and rule adherence.
- Ensure memory and repository‑aware reasoning behave deterministically.
- Confirm embedded AI capabilities function as expected.

## Structure

The structure of the `/tests` directory evolves alongside the `/src` directory. Each component added to `/src` includes a corresponding test project under `/tests`, following the naming conventions defined in the coding standards.

## Conventions

- Tests follow a clear Arrange‑Act‑Assert structure.
- All agent actions are validated through patch‑based assertions.
- Long‑running or multi‑step agent tests use deterministic seeds.
- New components include a matching test project.
