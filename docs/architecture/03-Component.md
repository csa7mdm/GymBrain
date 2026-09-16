# C4 Model - Component View

## Implemented Components

| Layer | Components |
|---|---|
| API | Auth, workout, nutrition, profile and telemetry endpoint groups; JWT authentication, CORS, exception middleware, liveness endpoints |
| Application | Feature-oriented MediatR commands/queries under Auth, Orchestration, Profile, Vault and Workout; FluentValidation pipeline; application service interfaces |
| Domain | User, Exercise, WorkoutSession, AnalyticsEvent, Milestone and UserMilestone entities; Result, Result<T>, Error; domain event contracts |
| Infrastructure | GymBrainDbContext and entity configurations; RedisCacheService with ResilientCacheService; RateLimiter; JWT, BCrypt and vault security services; HTTP LLM and ExerciseDB providers |

Application depends on Domain; Infrastructure depends on Application; API composes both. Domain references MediatR.Contracts. There is no implemented generic repository layer.

## Login Flow

1. POST `/api/auth/login` sends LoginUserCommand through MediatR and FluentValidation.
2. LoginUserCommandHandler queries the user and verifies the password.
3. The handler returns Result<LoginUserResponse>; success contains the existing userId/token response.
4. AuthEndpoints returns HTTP 200 or HTTP 401 with a detail field.

Registration similarly uses Result, returning HTTP 400 for an existing email. Other handlers still return DTOs or throw exceptions. FluentValidation throws ValidationException, mapped to HTTP 400 by middleware.

## Workout Flow

1. POST `/api/workout/start` invokes StartWorkoutCommandHandler.
2. Redis enforces an hourly limit before a cache lookup using `workout:{userId}:{experienceLevel}`.
3. A cache miss loads the user and exercise catalog, enforces managed usage limits if applicable, filters injuries, and calls an LLM provider.
4. SafetyGate validates the generated payload; deterministic warmups are prepended.
5. The generated workout is cached for two hours and returned as a response DTO.

Exercise metadata uses Workout/Queries/GetExerciseMetadataQuery and a 30-day cache. Nutrition uses Redis usage counters, but its generated meal payload is not cached or persisted by its handler.

## Operational Boundaries

Redis connects on demand. Payload cache operations tolerate connection/time-out failures, but usage counter failures return HTTP 503. PostgreSQL is required during startup migrations. `/health` reports liveness, not dependency readiness.

Serilog provides logging. The MediatR pipeline registers validation; separate logging/caching pipeline behaviors are not implemented. Domain-event-driven notifications and a full Result migration remain planned.

Tests cover Result invariants, authentication handlers, prompt/safety/injury logic, vault encryption and cache fallback. Auth tests use EF Core InMemory and test doubles; they do not prove PostgreSQL behavior. HTTP integration tests are planned.
