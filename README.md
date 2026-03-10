# GymBrain

GymBrain is an AI fitness coaching app with a .NET 9 Minimal API backend and a React + Vite frontend.

## Live Production

- Frontend: https://gymbrain-pilot-cairo.web.app/
- Backend: https://gymbrain-api-production.up.railway.app/
- Health: https://gymbrain-api-production.up.railway.app/health

## Stack

- Backend: .NET 9, Minimal APIs, EF Core, MediatR, Serilog
- Frontend: React 19, TypeScript, Vite
- Database: PostgreSQL
- Cache: Redis
- Hosting: Railway (API), Firebase Hosting (web)

## Repository Layout

```text
GymBrain/
|- src/
|  |- GymBrain.Api/
|  |- GymBrain.Application/
|  |- GymBrain.Domain/
|  `- GymBrain.Infrastructure/
|- tests/
|- client/
|- Dockerfile
`- GymBrain.sln
```

## Local Development

### Prerequisites

- .NET SDK 9+
- Node.js 20+
- Docker Desktop

### Backend

```powershell
docker compose up -d

dotnet user-secrets init --project src/GymBrain.Api
dotnet user-secrets set "Jwt:Secret" "replace-with-a-real-secret" --project src/GymBrain.Api
dotnet user-secrets set "Vault:EncryptionKey" "replace-with-base64-32-byte-key" --project src/GymBrain.Api

dotnet run --project src/GymBrain.Api
```

Default local API URLs:

- http://localhost:5000/
- http://localhost:5000/health
- http://localhost:5000/scalar/v1

### Frontend

```powershell
cd client
npm install
npm run dev
```

The frontend expects `VITE_API_URL` to point at the backend. For production builds, that value is stored in [`client/.env.production`](client/.env.production).

## Production Deployment

### Backend

The backend is deployed from the repository root to Railway using the root [`Dockerfile`](Dockerfile).

Canonical Railway variables:

- `ConnectionStrings__DefaultConnection`
- `REDIS_CONNECTION`
- `VAULT_ENCRYPTION_KEY`
- `JWT__Secret`
- `JWT__Issuer=GymBrain`
- `JWT__Audience=GymBrain`
- `ASPNETCORE_ENVIRONMENT=Production`
- `PORT=8080`

The API currently reads:

- Redis from `REDIS_CONNECTION` with fallback to `ConnectionStrings:Redis`
- Vault key from `VAULT_ENCRYPTION_KEY`
- JWT issuer/audience from `Jwt:Issuer` and `Jwt:Audience`

Allowed production CORS origins are defined in [`src/GymBrain.Api/appsettings.Production.json`](src/GymBrain.Api/appsettings.Production.json):

- `https://gymbrain-pilot-cairo.web.app`
- `https://gymbrain-pilot-cairo.firebaseapp.com`
- `https://gymbrain-api-production.up.railway.app`

### Frontend

The React app is built in [`client/`](client/) and deployed to Firebase Hosting.

```powershell
cd client
npm run build
firebase deploy --only hosting --project gymbrain-pilot-cairo
```

## Environment Files

- [`/.env.example`](.env.example): local development example
- [`/.env.template`](.env.template): production-oriented template
- [`/client/.env.production`](client/.env.production): production frontend API URL

Do not commit real secrets.

## Testing

Backend:

```powershell
dotnet test GymBrain.sln
```

Frontend:

```powershell
cd client
npm run build
```

Playwright:

```powershell
cd client
npx playwright test
```

## Security Notes

- No secrets should be stored in source-controlled appsettings files.
- API keys are encrypted before storage.
- Frontend request logging of credentials and vaulted API keys has been removed for production safety.
- Rotate any secret that was ever exposed in screenshots, logs, or committed files.

## Notes

- `Program.cs` only seeds an admin if `SeedAdmin:Email` and `SeedAdmin:Password` are explicitly configured.
- The live backend health endpoint is the best quick readiness check after deploy.
- If Railway variables are changed in bulk, prefer one controlled redeploy after updates.
