# ADR 002: Use FluentValidation with MediatR Pipeline Behaviors

## Status
Accepted

## Context
The GymBrain API uses MediatR for handling commands and queries. We needed a way to automatically validate incoming requests without repeating validation code in every handler. Previously, validation was done manually in some handlers, leading to inconsistent validation and potential missed validations.

## Decision
We will implement a MediatR pipeline behavior that runs FluentValidation validators before handing off to the actual handler. This ensures:
- All commands and queries are validated if a validator exists
- Validation errors are returned as `Result.Failure` with validation errors
- Handlers can assume the request is valid (fail-fast principle)
- Validation logic is centralized and reusable
- Consistent validation error format across all endpoints

## Consequences
### Positive
- Eliminates boilerplate validation code in handlers
- Ensures consistent validation across all commands/queries
- Makes validation errors explicit in the Result type
- Allows testing validation separately from business logic
- Follows the Single Responsibility Principle (handlers focus on business logic)
- Enables easy addition of new validators without modifying handlers

### Negative
- Slight increase in complexity due to MediatR pipeline
- Requires creating a validator for each command/query (but this is good practice)
- Need to register validators with the DI container
- Validation behavior runs for every MediatR call (negligible performance impact)

## Implementation
1. Created `ValidationBehavior<TRequest, TResponse>` in `/src/GymBrain.Application/Common/Behaviors/ValidationBehavior.cs`
2. Updated `DependencyInjection.cs` to scan and register all validators from the assembly
3. The behavior catches `ValidationException` from FluentValidation and converts it to a `Result.Failure` with validation errors

## Related Decisions
- ADR 001: Use Result Pattern for Error Handling
- ADR 003: Implement vertical slice architecture for features

## References
- [FluentValidation MediatR Pipeline](https://docs.fluentvalidation.net/en/latest/aspnet.html#aspnet-core-3)
- [MediatR Pipeline Behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors)