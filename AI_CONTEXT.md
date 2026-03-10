# GymBrain — AI/LLM Context File
<!--
  LAST UPDATED: 2026-03-10T05:40:00+02:00
  UPDATED BY: Codex
-->

## Project Overview

GymBrain is an AI fitness coaching platform with a production React frontend on Firebase Hosting and a production .NET 9 API on Railway.

- Backend: .NET 9 Minimal API
- Frontend: React + TypeScript + Vite
- Database: PostgreSQL
- Cache: Redis
- Production frontend: https://gymbrain-pilot-cairo.web.app/
- Production backend: https://gymbrain-api-production.up.railway.app/
- Production health: https://gymbrain-api-production.up.railway.app/health

## Current State

### Backend

- Railway deployment is active.
- Health endpoint is responding healthy.
- Redis is read from `REDIS_CONNECTION`.
- Vault encryption key is read from `VAULT_ENCRYPTION_KEY`.
- JWT issuer and audience are both configurable.
- Admin seeding only happens when `SeedAdmin:Email` and `SeedAdmin:Password` are explicitly set.

### Frontend

- Firebase Hosting project: `gymbrain-pilot-cairo`
- Production API base URL is stored in `client/.env.production`.
- Sensitive request/response console logging was removed from `client/src/services/api.ts`.

## Deployment Notes

### Railway

Canonical variables:

- `ConnectionStrings__DefaultConnection`
- `REDIS_CONNECTION`
- `VAULT_ENCRYPTION_KEY`
- `JWT__Secret`
- `JWT__Issuer`
- `JWT__Audience`
- `ASPNETCORE_ENVIRONMENT`
- `PORT`

### Firebase Hosting

- Config file: `client/firebase.json`
- Project binding: `client/.firebaserc`
- Deploy command: `firebase deploy --only hosting --project gymbrain-pilot-cairo`

## Important Constraints

- Do not store real secrets in `.env.example`, `.env.template`, or appsettings files.
- Do not reintroduce frontend logging of passwords, JWTs, or BYO API keys.
- If Railway variables are changed in bulk, prefer a single controlled redeploy after cleanup.
- Verify `/health` after each backend deploy.

## Recommended Next Checks

1. Open the production frontend in a browser.
2. Register a new test account.
3. Confirm there are no CORS errors in DevTools.
4. Confirm the registration row lands in the production database.
