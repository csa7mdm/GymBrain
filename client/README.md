# GymBrain Web Client

React + TypeScript + Vite frontend for GymBrain.

## Production

- Live URL: https://gymbrain-pilot-cairo.web.app/
- Production API: https://gymbrain-api-production.up.railway.app/

The production API base URL is configured in [`./.env.production`](./.env.production).

## Development

```powershell
npm install
npm run dev
```

Default dev server:

- http://localhost:5173/

## Build

```powershell
npm run build
```

## Deploy

```powershell
firebase deploy --only hosting --project gymbrain-pilot-cairo
```

## Key Files

- `src/services/api.ts`: typed API client
- `src/pages/`: top-level app screens
- `src/components/`: reusable UI pieces
- `firebase.json`: Firebase Hosting config
- `.firebaserc`: Firebase project binding

## Notes

- Do not add console logging that prints credentials, JWTs, or user-provided API keys.
- Keep `VITE_API_URL` pointed at Railway for production builds.
