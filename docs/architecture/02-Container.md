# C4 Model - Container Diagram

## Container Diagram: GymBrain Application

### Containers

#### 1. **Single Page Application (SPA)**
- **Technology**: React 19, TypeScript 5, Vite
- **Description**: Provides the user interface for users to interact with the GymBrain platform.
  - Views: Dashboard, Workout Plans, Nutrition Plans, Exercise Library, Profile, Settings
  - Features: Real-time updates, form validation, responsive design
- **Responsibilities**:
  - Present information to users
  - Handle user interactions
  - Communicate with the API via REST/JSON
  - Manage client-side state
  - Cache data for offline capability (future)

#### 2. **API Application**
- **Technology**: .NET 9, ASP.NET Core Minimal API, C#
- **Description**: The backend API that handles all business logic, data access, and external integrations.
  - Endpoints: Auth, Workout, Nutrition, Profile, Telemetry, Vault (for API keys)
  - Features: JWT authentication, validation, caching, logging, error handling
- **Responsibilities**:
  - Expose RESTful API for the SPA
  - Implement business logic (CQRS, domain services)
  - Manage data persistence (PostgreSQL via EF Core)
  - Integrate with external services (LLM providers, exercise metadata)
  - Handle authentication and authorization
  - Provide health checks and monitoring endpoints

#### 3. **PostgreSQL Database**
- **Technology**: PostgreSQL 15 (hosted on Neon.tech)
- **Description**: Relational database for storing all application data.
  - Schemas: Users, Workouts, Nutrition, Telemetry, Configuration
- **Responsibilities**:
  - Persist user profiles, workout plans, nutrition plans, completion records
  - Support ACID transactions
  - Provide querying capabilities for reports and analytics
  - Ensure data integrity and security

#### 4. **Redis Cache**
- **Technology**: Redis 7 (hosted on Upstash)
- **Description**: In-memory data store for caching frequently accessed data.
  - Used for: Exercise metadata, user profiles, computed recommendations
- **Responsibilities**:
  - Reduce database load by caching read-heavy operations
  - Improve response times for API endpoints
  - Store temporary session data (if needed)
  - Support automatic expiration of cached data

#### 5. **LLM Provider Services**
- **Technology**: Various (OpenAI API, Groq API, OpenRouter API)
- **Description**: External services providing AI capabilities for personalized recommendations.
  - Services: GPT-4/Turbo, Llama 3 via Groq, various models via OpenRouter
- **Responsibilities**:
  - Generate personalized workout and nutrition recommendations
  - Provide exercise form feedback and tips
  - Adapt plans based on user feedback and progress
  - Power the AI coaching chatbot (future)

#### 6. **Exercise Metadata Service**
- **Technology**: Custom API or third-party service (to be defined)
- **Description**: Service providing detailed exercise information.
  - Data: Exercise names, descriptions, muscle groups, equipment needed, difficulty level
  - Media: Images, videos demonstrating proper form
- **Responsibilities**:
  - Supply comprehensive exercise library data
  - Provide metadata for workout plan generation
  - Support filtering by muscle group, equipment, difficulty

### Container Interactions

1. **SPA ↔ API Application**
   - Communication: HTTPS/JSON (RESTful API)
   - Protocols: GET, POST, PUT, DELETE for CRUD operations
   - Authentication: JWT Bearer tokens in Authorization header
   - Data Exchange: User profiles, workout plans, nutrition plans, exercise data

2. **API Application ↔ PostgreSQL Database**
   - Communication: Entity Framework Core (ADO.NET provider for Npgsql)
   - Operations: Queries, commands, transactions
   - Data Flow: Read user profiles, write workout completions, update plans

3. **API Application ↔ Redis Cache**
   - Communication: StackExchange.Redis client
   - Operations: GET, SET, INCREMENT, KEYEXPIRE
   - Data Flow: Cache exercise metadata, cache user profile lookups, cache computed recommendations

4. **API Application ↔ LLM Provider Services**
   - Communication: HTTP/JSON via HttpClient (with Polly for resilience)
   - Operations: POST requests to generate completions
   - Data Flow: Send prompts (user context + goals), receive AI-generated plans/advice

5. **API Application ↔ Exercise Metadata Service**
   - Communication: HTTP/JSON or gRPC
   - Operations: GET requests for exercise details
   - Data Flow: Request exercise metadata, receive detailed exercise information

6. **External Authentication** (Future)
   - Communication: OAuth 2.0/OpenID Connect flows
   - Data Flow: Redirect to provider, receive tokens, validate and create local user

### Deployment Notes
- **SPA**: Hosted on Firebase Hosting (with custom domain: gymbrain-pilot-cairo.web.app)
- **API Application**: Hosted on Railway (with custom domain: gymbrain-api-production.up.railway.app)
- **PostgreSQL**: Hosted on Neon.tech (serverless PostgreSQL)
- **Redis**: Hosted on Upstash (serverless Redis)
- **LLM Services**: Various third-party APIs (OpenAI, Groq, OpenRouter)
- **Exercise Metadata**: To be implemented (potentially custom service or third-party)

### Security Considerations
- All inter-service communication over HTTPS
- JWT tokens are short-lived (15 minutes) with refresh tokens stored securely (HTTP-only cookies planned)
- API keys for external services stored as environment variables/secrets
- Database connections use SSL/TLS
- Rate limiting on API endpoints to prevent abuse