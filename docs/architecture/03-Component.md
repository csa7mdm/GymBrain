# C4 Model - Component Diagram

## Component Diagram: GymBrain API Application

### Components within the API Application Container

#### 1. **API Layer (Endpoints)**
- **Technology**: ASP.NET Core Minimal API
- **Location**: `/src/GymBrain.Api/Endpoints/`
- **Components**:
  - `AuthEndpoints.cs` - Authentication (register, login, vault API keys)
  - `WorkoutEndpoints.cs` - Workout plan retrieval, completion logging
  - `NutritionEndpoints.cs` - Nutrition plan retrieval, meal logging
  - `ProfileEndpoints.cs` - User profile management
  - `TelemetryEndpoints.cs` - Usage analytics and performance metrics
- **Responsibilities**:
  - Define HTTP endpoints (routes, HTTP methods, request/response types)
  - Handle authentication and authorization requirements
  - Mediate between HTTP requests and application commands/queries
  - Return appropriate HTTP status codes based on operation results
  - Apply cross-cutting concerns (validation, logging) via middleware

#### 2. **Application Layer**
- **Technology**: MediatR, C#
- **Location**: `/src/GymBrain.Application/` (organized by feature)
- **Components**:
  - **Commands** (Write Operations):
    - `LoginUserCommand` / `LoginUserCommandHandler`
    - `RegisterUserCommand` / `RegisterUserCommandHandler`
    - `CompleteWorkoutCommand` / handler (example)
    - `UpdateProfileCommand` / handler (example)
  - **Queries** (Read Operations):
    - `GetProfileQuery` / `GetProfileQueryHandler`
    - `GetWorkoutPlanQuery` / handler (example)
    - `GetNutritionPlanQuery` / handler (example)
    - `GetExerciseMetadataQuery` / `GetExerciseMetadataQueryHandler`
  - **Common Infrastructure**:
    - `ValidationBehavior` - Pipeline behavior for automatic FluentValidation
    - `Interfaces` - Application service contracts (ICacheService, IJwtTokenService, etc.)
    - `Exceptions` - Custom exception types for domain-specific errors

#### 3. **Domain Layer**
- **Technology**: C#
- **Location**: `/src/GymBrain.Domain/`
- **Components**:
  - **Entities**:
    - `User.cs` - Core user aggregate root
    - `Workout.cs` - Individual workout session
    - `NutritionPlan.cs` - Daily/weekly nutrition guidance
    - `Exercise.cs` - Exercise definition with metadata
    - `Milestone.cs` - Achievement/tracking milestones
  - **Value Objects**:
    - To be implemented: `Email.cs`, `Password.cs`, `Weight.cs`, etc.
  - **Enums**:
    - `ExperienceLevel.cs` - Beginner, Intermediate, Advanced
    - `GoalType.cs` - WeightLoss, MuscleGain, Endurance, Strength
    - `EquipmentType.cs` - Dumbbells, Bands, Bodyweight, etc.
  - **Common**:
    - `Result.cs` - Result monad for error handling without exceptions
    - `BaseEntity.cs` - Base class with Id, CreatedAt, UpdatedAt
    - `DomainEvents.cs` - Base class for domain events
    - `Interfaces.ts` - Domain service contracts

#### 4. **Infrastructure Layer**
- **Technology**: Various (EF Core, Redis, HTTP clients)
- **Location**: `/src/GymBrain.Infrastructure/`
- **Components**:
  - **Persistence**:
    - `GymBrainDbContext.cs` - EF Core DbContext
    - `Configurations/` - EntityTypeConfiguration for each entity
    - `Repositories/` - Implementation of domain repository interfaces
  - **Services**:
    - `RedisCacheService.cs` - Implementation of ICacheService
    - `JwtTokenService.cs` - Implementation of IJwtTokenService
    - `PasswordHasher.cs` - Implementation of IPasswordHasher (BCrypt)
    - `RateLimiter.cs` - Implementation of IRateLimiter
    - `VaultService.cs` - Management of external API keys
    - `LlmProviderFactory.cs` - Factory for creating LLM provider instances
    - Individual LLM providers: `OpenAiProvider.cs`, `GroqProvider.cs`, etc.
    - `ExerciseMetadataProvider.cs` - Implementation of IExerciseMetadataService
  - **Providers**:
    - HTTP client configurations for external services
  - **Security**:
    - Additional security-related services
  - **DependencyInjection.cs** - Extension methods for registering services

#### 5. **Cross-Cutting Concerns**
- **Logging**: Serilog integration (configured in Program.cs)
- **Validation**: FluentValidation with MediatR pipeline behaviors
- **Caching**: Decorator pattern or direct service injection
- **Error Handling**: Middleware for converting exceptions to HTTP responses
- **Health Checks**: Endpoints for liveness/readiness probes

### Component Interactions

#### Request Flow Example: User Login
1. **SPA** → POST `/api/auth/login` (Email, Password)
2. **AuthEndpoints** receives request, creates `LoginUserCommand`
3. **MediatR** routes command to `LoginUserCommandHandler`
4. **Handler** validates credentials via:
   - `IApplicationDbContext` (EF Core) to fetch user by email
   - `IPasswordHasher` to verify password hash
   - `IJwtTokenService` to generate JWT token
5. **Handler** returns `Result<LoginUserResponse>` (success/failure)
6. **AuthEndpoints** maps result to:
   - 200 OK with token (success)
   - 401 Unauthorized with error message (failure)
7. **SPA** receives response, stores token for subsequent requests

#### Request Flow Example: Get Workout Plan (with Caching)
1. **SPA** → GET `/api/workout/plan/{userId}`
2. **WorkoutEndpoints** creates `GetWorkoutPlanQuery` with userId
3. **MediatR** routes to `GetWorkoutPlanQueryHandler`
4. **Handler** checks cache via `ICacheService` for key `workout_plan:{userId}`
5. **If cache miss**:
   - Handler retrieves user profile from `IApplicationDbContext`
   - Generates workout plan based on user goals, equipment, experience
   - May consult `IExerciseMetadataService` for exercise details
   - Handler caches result with TTL (e.g., 24 hours)
6. **Handler** returns workout plan (wrapped in Result)
7. **WorkoutEndpoints** maps to 200 OK or appropriate error
8. **SPA** receives and displays workout plan

### Dependency Flow
- **API Layer** depends on: MediatR (for sending commands/queries)
- **Application Layer** depends on: Domain interfaces, MediatR abstractions
- **Domain Layer** has: Zero external dependencies (pure business logic)
- **Infrastructure Layer** implements: Domain and application interfaces
- **Composition Root** (`DependencyInjection.cs`): Wire up all implementations

### Key Architectural Decisions Visible in Components
1. **Separation of Concerns**: Clear layers with unidirectional dependencies
2. **CQRS**: Commands and queries are separate objects handled by different handlers
3. **Dependency Injection**: All dependencies are injected via constructors
4. **Interface Segregation**: Small, focused interfaces (ICacheService, IJwtTokenService, etc.)
5. **Result Pattern**: Functional error handling without exceptions for expected outcomes
6. **Validation Pipeline**: Automatic validation via MediatR behavior
7. **Caching Abstraction**: Cache service injected where needed, Redis implementation hidden
8. **External Service Abstraction**: LLM providers, exercise metadata behind interfaces

### Component Communication Patterns
- **Synchronous**: Direct method calls (most application flow)
- **Pipeline**: MediatR behaviors (validation, logging, caching decorators)
- **Event-Driven** (Future): Domain events triggering side effects (notifications, analytics)
- **Async/Await**: All I/O operations (database, cache, HTTP calls) are asynchronous

### Technology Justification Visible in Components
- **.NET 9**: Top-level statements, minimal APIs, enhanced JSON serialization
- **MediatR**: Decouples senders from handlers, enables pipeline behaviors
- **FluentValidation**: Fluent, readable validation rules
- **Serilog**: Structured logging with rich enrichment capabilities
- **EF Core**: Object-relational mapper with LINQ support
- **StackExchange.Redis**: High-performance Redis client
- **HttpClient Factory**: Typed clients for external services with resilience policies
- **xUnit/Moq** (Testing): Modern testing framework with mocking library

This component diagram shows how the GymBrain API applies modern .NET architectural patterns to create a maintainable, testable, and scalable system suitable for demonstrating senior-level .NET expertise.