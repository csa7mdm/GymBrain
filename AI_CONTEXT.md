# GymBrain — AI/LLM Context File
<!-- 
  This file is designed to be consumed by AI/LLM agents.
  It provides a machine-readable, continuously updated snapshot of 
  the project's current state, architecture, and progress.
  
  LAST UPDATED: 2026-03-05T04:30:00+02:00
  UPDATED BY: Antigravity AI Agent
-->

## Project Overview

**GymBrain** is an AI-powered fitness coaching platform that generates personalized, 
real-time workout plans using LLM orchestration with a Server-Driven UI architecture.

- **Backend:** .NET 9 Minimal API (Clean Architecture)
- **Frontend (Demo):** React + TypeScript + Vite
- **Frontend (Production):** Flutter (planned, not yet built)
- **Database:** PostgreSQL 16 + Redis 7 (Docker)
- **AI Engine:** Multi-Provider (OpenAI, Groq, OpenRouter, Anthropic)
- **Cost Target:** $0.01 - $0.05 USD per session (Free tier support via Groq/OpenRouter)

---

## Current Implementation Status

### Backend (.NET 9) — ✅ COMPLETE

| Layer | Status | Key Files |
|-------|--------|-----------|
| **Domain** | ✅ Complete | `BaseEntity.cs`, `Result.cs`, `ValueObject.cs`, `User.cs`, `Exercise.cs`, `IVaultService.cs` |
| **Application** | ✅ Complete | Auth, Vault, Orchestration, `LlmModelCatalog.cs`, Nutrition generation |
| **Infrastructure** | ✅ Complete | VaultService, JwtTokenService, BcryptPasswordHasher, DbContext, Providers (OpenAI, Groq, OpenRouter, Anthropic), `LlmProviderFactory` |
| **API** | ✅ Complete | AuthEndpoints, WorkoutEndpoints (Save/Start), NutritionEndpoints, Program.cs with Scalar docs |
| **Tests** | ✅ 22/22 passing | Result, SafetyGate, SystemPromptFactory, VaultService tests |

### Frontend (React Demo) — ✅ COMPLETE

| Component | Status | Description |
|-----------|--------|-------------|
| Project scaffolding | ✅ Complete | Vite + React + TypeScript |
| Design system | ✅ Complete | Dark OLED theme, glassmorphism, Inter/Roboto Mono |
| Auth screens | ✅ Complete | Register, Login with tone persona selection |
| Vault screen | ✅ Complete | Multi-provider selector (Groq, OpenRouter, etc.) + model catalog dropdown |
| Workout screen | ✅ Complete | SDUI renderer (tone_card, set_tracker, fallback) + Local save offline/sync |
| Nutrition / Profile | ✅ Complete | Dietary preferences form, Goal selection, AI Meal Plan generation UI |
| Analytics / Plans | ✅ Complete | Health Pillars (streak/completion), Cycle Progression visualizer |
| API service | ✅ Complete | Configurable base URL, JWT injection, typed responses |

### Flutter (Production) — ⏳ DEFERRED

| Component | Status | Notes |
|-----------|--------|-------|
| All components | ⏳ Deferred | Will be built after React demo validates the backend |

### DevOps — ✅ COMPLETE

| Component | Status |
|-----------|--------|
| Docker Compose | ✅ PostgreSQL 16 + Redis 7 |
| Git repository | ✅ Initialized |
| GitHub remote | ✅ Pushed to github.com/csa7mdm/GymBrain |
| User Secrets | ✅ Configured (Jwt:Secret, Vault:EncryptionKey) |

---

## Architecture Decisions Record (ADR)

### ADR-001: Token Compression Strategy
- **Decision:** Compress exercises to `ID|Name` format in LLM prompts
- **Rationale:** Full exercise objects (description, muscle, category) waste tokens without improving output quality
- **Impact:** Estimated 70% token reduction per prompt

### ADR-002: Safety Gate Pattern
- **Decision:** Post-process LLM JSON output in C# instead of re-prompting
- **Rationale:** Re-prompting doubles cost. C# can deterministically fix hallucinated IDs and clamp weights
- **Impact:** Zero additional LLM calls for safety enforcement

### ADR-003: Secrets Management
- **Decision:** Never store secrets in appsettings.json
- **Rationale:** Previous implementation leaked JWT secrets and vault keys in source control
- **Impact:** All secrets via User Secrets (dev) or env vars (production)

### ADR-004: Exercise Seeding
- **Decision:** Seed 65 exercises in EF Core OnModelCreating with deterministic GUIDs
- **Rationale:** Expanded from 15 → 65 to ensure full muscle group coverage and equipment-based filtering.
- **Impact:** Backend supports full auditing remediation immediately after migration.

### ADR-005: React Demo First, Flutter Later
- **Decision:** Build React web demo app before Flutter mobile app
- **Rationale:** Faster iteration cycle, validates backend API contract before mobile investment
- **Impact:** Flutter deferred to next phase

### ADR-006: Pre-flight Health Check & Model Fallback
- **Decision:** Validate API key + model via minimal LLM call (maxTokens=1) before vaulting
- **Rationale:** Prevents broken keys from being stored; auto-falls back to other free models if preferred is unavailable
- **Impact:** Zero broken keys in production; seamless user experience with auto-switching

---

## API Contract

### Authentication
```
POST /api/auth/register
Body: { "email": "string", "password": "string", "tonePersona": "string?" }
Response: { "userId": "guid", "token": "jwt-string" }

POST /api/auth/login
Body: { "email": "string", "password": "string" }
Response: { "userId": "guid", "token": "jwt-string" }

POST /api/auth/vault-key [Authorized]
Body: { "provider": "openai|groq|openrouter|anthropic", "apiKey": "string", "model": "string?" }
Response: { "message": "API key verified and vaulted for groq (llama-3.3-70b-versatile)." }

NOTE: The vault-key endpoint now performs a health check before vaulting.
It sends a minimal request (maxTokens=1, forceJson=false) to verify the key and model.
If the preferred model fails, it auto-falls back to other free models.
The response message indicates which model was ultimately used.

GET /api/auth/models
Response: Array of LlmModelInfo { provider, modelId, displayName, description, isFree, suitabilityRank }
```

### Workout
```
POST /api/workout/start [Authorized]
Body: { "workoutFocus": "string?" }
Response: { "megaPayloadJson": "SDUI JSON string" }

POST /api/workout/save [Authorized]
Body: { "payloadJson": "SDUI JSON string" }
Response: { "workoutSessionId": "guid" }
```

### Nutrition
```
POST /api/nutrition/generate [Authorized]
Body: { "diet": "string", "calories": "int", "goal": "string" }
Response: { "payloadJson": "SDUI JSON string" }
```

### Health
```
GET /
Response: "GymBrain API is alive"

GET /health
Response: { "status": "healthy", "timestamp": "datetime" }
```

---

## SDUI Mega-Payload Schema (Expected LLM Output)

```json
{
  "screen_id": "active_workout",
  "schema_version": "1.0",
  "theme": "dark_oled",
  "components": [
    {
      "type": "tone_card",
      "payload": {
        "message": "Let's crush it today!",
        "persona": "Drill Sergeant"
      }
    },
    {
      "type": "set_tracker",
      "payload": {
        "exercise_id": "10000001-0000-0000-0000-000000000001",
        "exercise_name": "Barbell Squat",
        "sets": 3,
        "reps": 10,
        "weight_kg": 30.0,
        "rest_seconds": 90
      }
    }
  ]
}
```

---

## Seeded Exercise Catalog

| ID | Name | Target Muscle | Category | Beginner Weight |
|----|------|---------------|----------|-----------------|
| `...001` | Barbell Squat | Quadriceps | Compound | 20kg |
| `...002` | Barbell Deadlift | Back | Compound | 20kg |
| `...003` | Barbell Bench Press | Chest | Compound | 20kg |
| `...004` | Overhead Press | Shoulders | Compound | 15kg |
| `...005` | Barbell Row | Back | Compound | 20kg |
| `...006` | Pull-Up | Back | Bodyweight | 0kg |
| `...007` | Dumbbell Lunges | Quadriceps | Compound | 10kg |
| `...008` | Dumbbell Curl | Biceps | Isolation | 8kg |
| `...009` | Tricep Pushdown | Triceps | Isolation | 10kg |
| `...010` | Leg Press | Quadriceps | Compound | 40kg |
| `...011` | Lat Pulldown | Back | Compound | 25kg |
| `...012` | Cable Fly | Chest | Isolation | 10kg |
| `...013` | Plank | Core | Bodyweight | 0kg |
| `...014` | Romanian Deadlift | Hamstrings | Compound | 20kg |
| `...015` | Lateral Raise | Shoulders | Isolation | 5kg |

---

## File Tree (Key Files Only)

```
d:\workspace\
├── .antigravityrules              ← AI agent instructions
├── .gymbrain_knowledge.md         ← Living log of faults & lessons learned
├── AI_CONTEXT.md                  ← THIS FILE (LLM-friendly project state)
├── README.md                      ← Human-readable documentation
├── GymBrain.sln
├── Directory.Build.props
├── docker-compose.yml
├── .env.example
│
├── src/
│   ├── GymBrain.Domain/
│   │   ├── Common/                ← BaseEntity, IDomainEvent, ValueObject, Result<T>
│   │   ├── Entities/              ← User, Exercise
│   │   ├── Enums/                 ← ExperienceLevel
│   │   └── Interfaces/            ← IVaultService
│   ├── GymBrain.Application/
│   │   ├── Auth/Commands/         ← Register, Login handlers
│   │   ├── Vault/Commands/        ← VaultApiKey handler
│   │   ├── Orchestration/         ← SystemPromptFactory, SafetyGate
│   │   │   └── Commands/          ← StartWorkout handler
│   │   └── Common/Interfaces/     ← IApplicationDbContext, ICacheService, ILlmProvider, etc.
│   ├── GymBrain.Infrastructure/
│   │   ├── Security/              ← VaultService, JwtTokenService, BcryptPasswordHasher
│   │   ├── Persistence/           ← DbContext, Seeder, Configurations
│   │   ├── Providers/             ← OpenAiProvider
│   │   └── Services/              ← RedisCacheService
│   └── GymBrain.Api/
│       ├── Endpoints/             ← AuthEndpoints, WorkoutEndpoints
│       └── Program.cs
│
├── tests/
│   ├── GymBrain.Domain.Tests/
│   ├── GymBrain.Application.Tests/
│   ├── GymBrain.Infrastructure.Tests/
│   └── GymBrain.Api.Tests/
│
└── client/                        ← React demo app (Vite + TypeScript)
    └── (pending)
```

---

## Environment Setup

```bash
# 1. Start infrastructure
docker compose up -d

# 2. Configure secrets (one-time, dev only)
dotnet user-secrets set "Jwt:Secret" "your-32-char-secret" --project src/GymBrain.Api
dotnet user-secrets set "Vault:EncryptionKey" "base64-encoded-32-byte-key" --project src/GymBrain.Api

# 3. Create database migration
dotnet ef migrations add InitialCreate --project src/GymBrain.Infrastructure --startup-project src/GymBrain.Api

# 4. Run the API
dotnet run --project src/GymBrain.Api

# 5. Run tests
dotnet test GymBrain.sln

# 6. Run React demo
cd client && npm run dev
```

---

## Continuation Instructions for AI Agents

When resuming work on this project:

1. **Read `.antigravityrules`** first for all coding conventions and constraints
2. **Read this file (`AI_CONTEXT.md`)** for current state and what's been done
3. **Check the "Current Implementation Status" section** above for what's pending
4. **Never hardcode secrets** — always use User Secrets or env vars
5. **Always run `dotnet build` and `dotnet test`** after changes
6. **Commit with conventional commits** (`feat:`, `fix:`, `docs:`, etc.)
7. **Update this file** after completing any significant work
