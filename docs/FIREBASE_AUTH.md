# Firebase sign-in rollout

The production client uses the official Firebase SDK for Google popup sign-in, email/password accounts, email verification, and password-reset emails. The public Firebase web configuration in client/src/services/firebase.ts (overridable with VITE_FIREBASE_* variables) is not an admin credential. No service account private key is required. Keep the project on Spark for the requested no-cost authentication setup; email quotas still apply.

The API validates Firebase RS256 signatures using Google's HTTPS OpenID discovery/JWKS, pins issuer and audience to Firebase:ProjectId, and validates expiry, issued/authentication times and verified email. Firebase tokens are refreshed by the browser SDK before API calls. The project currently enables email/password and Google providers.

## Existing accounts

- A new verified Firebase identity does not automatically receive access to an existing account with a matching email.
- The linking screen requires the current GymBrain password and explicitly explains the switch. Matching is case insensitive; ambiguous legacy emails fail closed.
- The existing GUID, workout history, profile, and password hash remain intact. A unique FirebaseUid column records the link, with optimistic concurrency protection.
- After linking, legacy password login and previously issued legacy JWTs are rejected. The retained hash is for a controlled rollback, not an alternate live sign-in route.
- Before linking, Previous account sign-in continues to work. Users who forgot an unlinked legacy password need an independently verified assisted-recovery process; Firebase reset alone cannot recover those accounts. Do not claim universal legacy recovery is complete.
- Google-only users should use Google sign-in; Firebase password recovery applies to password-enabled Firebase accounts.

## Deploy

1. Run backend tests, frontend lint/build and desktop/mobile release tests.
2. Deploy the API first. The additive AddFirebaseIdentity migration adds one nullable column and unique index. It neither exports nor rewrites existing passwords.
3. Check Firebase Authentication authorized domains include the exact frontend hostname. Preview hostnames need separate authorization for Google sign-in. No Android SHA fingerprint is needed for this web app.
4. Deploy the frontend, then test a real Google sign-in, email verification/reset, logout/reload, and explicit legacy linking using authorized test accounts. Mock tests do not demonstrate live Google consent or email delivery.
5. Keep Firebase email templates on the default sender/domain. A custom domain or Resend account is not required.

## Operational limits

- Firebase ID tokens already issued can remain valid until expiry (normally one hour) after account disablement/password reset. This implementation validates tokens locally; it does not perform a per-request Firebase revocation lookup. Immediate global revocation remains a release gate if required.
- The API auth endpoints have a conservative 20 requests/minute limiter per observed remote IP. Behind a proxy, verify trusted forwarded-header configuration before public scale; callers sharing the proxy IP may share a bucket. Firebase also applies its own abuse limits.
- A rollback to a backend version unaware of FirebaseUid would reactivate retained old password hashes and reject Firebase tokens. Coordinate rollback and explicitly assess this security consequence; do not blindly downgrade after accounts link.
- Privacy disclosures must explain Firebase/Google authentication. Firebase stores authentication data; workouts remain in GymBrain's database.

## Verification

Backend tests use the real JWT validation pipeline and locally generated RSA keys, rejecting wrong audience/issuer/signature, unsigned/expired tokens, future authentication time and unverified email. Tests assert explicit password proof, original account preservation, old-session rejection and conflicting UID denial.

Browser tests use the actual Firebase SDK against intercepted HTTP endpoints: account-linking flow, reload, verification gate, neutral reset response, and blocked Google popup. They send no emails and create no production accounts.
