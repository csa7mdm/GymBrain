# Release readiness — 2026-09-17

Status: core workout reliability candidate. Public launch is not approved by these checks alone.

## Implemented

- Completed workouts load from authenticated server history, including on another browser. The History tab replaces the nonpersistent plan builder.
- New clients create one session ID per workout. Completion stores actual per-set reps, weight, and completion state. First successful save is immutable; retries do not add records or completion counts. Database insert/counter update is atomic, with optimistic concurrency retries for simultaneous sessions.
- Failed/ambiguous saves keep the exact attempted payload for retry. Progress and IDs survive reloads in the same browser tab, scoped by account. Drafts are not synchronized between devices; closing the tab can discard an unfinished draft.
- Account startup reads saved training preferences from the server. A returning account with a goal skips onboarding even without local storage. Display name, age, height, weight, and focus-area metadata still need server persistence.
- Generation reads current profile level. Cache identity includes focus, level, goal, equipment, restrictions, tone, progression count, and provider configuration.
- Generation and substitutions enforce equipment and existing injury keyword exclusions against catalog IDs. Unknown exercise IDs and malformed component/numeric shapes are rejected; displayed exercise names come from the catalog. This is not clinical validation of exercise programming or comprehensive injury support.
- Workout and substitution limits are enforced once per request, with separate hourly-limit messages and Retry-After. Cache hits count toward the hourly allowance; managed uncached generation attempts consume the daily allowance, including failed attempts. Generation times out after 40 seconds; browser requests time out after 45 seconds.
- Client-only substitution fallbacks were removed. Operator metrics and the arbitrary completion-counter increment route are no longer publicly mapped. Unsupported Anthropic models are excluded from the public model list.

## Verification

`dotnet test GymBrain.sln --configuration Release` covers domain/application validation, SQLite relational completion races, immutable retry payloads, history ownership/pagination, current-profile caching, and a real HTTP register/login/profile/generate/save/login/history journey. The HTTP test uses real JWT authentication and the production application pipeline with a disposable SQLite database and controlled AI/cache adapters. A model comparison checks the PostgreSQL migration snapshot without opening a production database.

`npm run build`, `npm run lint`, and `npm run test:release` in `client` cover desktop/mobile onboarding retries, credential cleanup, exercise metadata failures, substitution progress, actual set recording, interrupted-save retry, reload, and a fresh browser viewing saved history. Browser API calls are mocked. These checks do not replace a deployed PostgreSQL/Redis/provider smoke test.

## Rollout and compatibility

1. Let backend CI, Docker build, and frontend checks pass for the exact commit.
2. Deploy the API first and check Railway deployment status plus health. No new schema migration is needed; the existing session primary key implements idempotency, and the existing user counter is configured as an EF concurrency token.
3. Deploy the same commit's `firebase-client` build artifact to a Firebase preview, then prove the full journey against a staging API/database before promoting hosting. A frontend artifact upload is not a Firebase deployment.
4. Record both deployed revisions. On rollback, coordinate frontend and backend versions: the old backend does not honor session IDs.

Old clients without session IDs remain compatible during rollout but are not duplicate-safe. Historical payloads lacking actual-set results are labeled as older records. Existing duplicate records and browser-only plans are not deleted or silently reinterpreted.

## Remaining launch gates

- Decide invite-only pilot versus open signup and configure admission accordingly.
- Complete password recovery with a verified sender/email service, account deletion/export, authentication abuse protection, and session lifecycle handling.
- Persist the remaining personal profile fields; remove or clearly defer incomplete nutrition/plan experiences.
- Verify the core journey on deployed PostgreSQL/Redis with a controlled provider and a budgeted real-provider smoke test.
- Have qualified fitness reviewers review representative routines, equipment metadata, restrictions, and advertised claims. The current keyword map is not comprehensive.
- Provide the product owner's privacy/terms/support details and data retention policy.
- Verify backup restoration and coordinated rollback; configure error, latency, and managed-AI spending alerts/ceilings.
- Deploy and verify the frontend, and run accessibility/keyboard and additional-browser checks.
