# ADR 001: Use Result Pattern for Error Handling

## Status
Accepted

## Context
The GymBrain API needed a consistent way to handle expected business errors (validation failures, not found, unauthorized access) without using exceptions for control flow. Previously, some handlers threw exceptions while others returned nullable values or special response types, leading to inconsistent error handling and potential 500 errors for expected business conditions.

## Decision
We will implement a Result pattern (similar to functional programming's Either monad) where:
- All command/query handlers return `Result<T>` or `Result` 
- `Result<T>.Success()` represents successful operations with a value
- `Result.Failure(Error)` represents failed operations with an error object
- Expected business errors (validation, not found, unauthorized) return Failure results
- Unexpected errors (database connection failures, null references) still throw exceptions
- API endpoints map Result outcomes to appropriate HTTP status codes:
  - Success → 200 OK
  - Validation failure → 400 Bad Request
  - Not found → 404 Not Found
  - Unauthorized → 401 Unauthorized
  - Forbidden → 403 Forbidden
  - Conflict → 409 Conflict
  - Unexpected errors → 500 Internal Server Error

## Consequences
### Positive
- Consistent error handling across all handlers
- Eliminates exceptions for expected business flows
- Makes success/failure paths explicit in method signatures
- Enables middleware-style error handling in API endpoints
- Improves testability (can assert on Result type rather than exception throwing)
- Better performance (no exception construction/stack unwinding for expected cases)
- Clear separation between expected business errors and unexpected system errors

### Negative
- Initial learning curve for developers unfamiliar with Result pattern
- Slightly more verbose than throwing exceptions (but more explicit)
- Need to map Result to HTTP responses in endpoints (mitigated by helper methods)
- Requires discipline to not throw exceptions for expected business conditions

## Implementation
Created `GymBrain.Domain.Common.Result` and `GymBrain.Domain.Common.Error` classes.
Updated `LoginUserCommandHandler` and `RegisterUserCommandHandler` to return `Result<T>`.
Updated `AuthEndpoints` to map Result outcomes to appropriate HTTP responses.

## Related Decisions
- ADR 002: Use FluentValidation with MediatR pipeline behaviors
- ADR 003: Implement vertical slice architecture for features

## References
- [Result Pattern in C#](https://ardalis.com/the-result-pattern-in-csharp/)
- [Functional Error Handling in C#](https://blog.ploeh.dk/2015/08/03/either-as-a-monad-for-error-handling-in-csharp/)