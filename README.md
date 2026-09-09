# CouponHub AI

Android coupon wallet with an ASP.NET Core 9 / PostgreSQL backend. Extends the existing Clean Architecture solution with accounts, private coupon imports, search, saved offers, personalized recommendations and an Android client.

## Start locally

Prerequisites: Docker with Compose; Android Studio with Android SDK 35, JDK 17 and Gradle 8.11.1 for the Android app.

1. Copy `.env.example` to `.env`. Set a long random `POSTGRES_PASSWORD` without semicolons.
2. Run `docker compose up --build -d`. The one-shot migration service applies migrations before the API starts.
3. Check `http://localhost:8080/health/ready` and open `http://localhost:8080/swagger`.
4. Create the initial administrator **before registering that email**:

   ```bash
   export COUPONHUB_ADMIN_EMAIL='your-admin@example.com'
   read -r -s -p 'Admin password (12–128 characters): ' COUPONHUB_ADMIN_PASSWORD
   export COUPONHUB_ADMIN_PASSWORD
   docker compose run --rm -e COUPONHUB_ADMIN_EMAIL -e COUPONHUB_ADMIN_PASSWORD api --create-admin
   unset COUPONHUB_ADMIN_PASSWORD
   ```

5. Open `android/` in Android Studio. Use Gradle 8.11.1, or run `gradle -p android wrapper --gradle-version 8.11.1` with a local Gradle installation before opening. A wrapper binary is not included.
6. Run on an emulator. Debug builds use `http://10.0.2.2:8080/`.
7. Sign in as administrator and add brands in **You → Manage brands**. Import offers and use **Terms & actions → Publish** for coupons intended for everyone. Regular users' imports stay private.

No fake merchant coupons or default passwords are seeded. Register a separate regular account to test private imports.

For a device or release build, configure an HTTPS API endpoint:

```bash
gradle -p android :app:assembleRelease -PAPI_BASE_URL=https://your-api.example.com/
```

Release builds reject cleartext traffic. The supplied Compose setup binds to loopback and runs in Development for local use; it is not an internet-facing deployment configuration. Deploy the API with HTTPS, Production environment, a managed secret store, backups and a configured host name. Use direct TLS termination in the app or configure trusted forwarded headers for your specific reverse proxy before enabling HTTPS redirects behind it.

## AI integration

Set `OPENAI_API_KEY` and `OPENAI_MODEL` in `.env`, then recreate the API container. Choose a Chat Completions model available to your account that supports JSON mode and `max_completion_tokens`. The model is deliberately configurable; there is no embedded API key. Server-side configuration names are `AI__ApiKey` and `AI__Model`.

- **Recommendations:** filter by ownership, active brand/coupon, expiry and minimum order. Rank up to 200 recent eligible offers by query terms, preferences, saved status and estimated benefit. If the user opts into AI, send the top 40 candidates for relevance ordering. Only IDs from that shortlist are accepted; the server retains authoritative coupon terms and savings calculations. AI failure returns a labeled rules fallback.
- **Extraction:** screenshot OCR runs on the Android device with ML Kit. The user reviews the text and explicitly opts into sending it to OpenAI. The response is an editable draft. The user selects an existing brand and submits through normal domain validation. Unknown fields stay empty.
- **No-key behavior:** accounts, manual imports, search, favorites and deterministic recommendations work; AI extraction returns 503 with a manual-entry option.
- JSON mode output is still treated as untrusted and validated. See [OpenAI JSON / structured output documentation](https://developers.openai.com/api/docs/guides/structured-outputs).

Search uses database text matching. AI ranking understands intent over a bounded shortlist; this implementation does **not** claim to provide a corpus-wide embedding/vector search index. Cashback is estimated benefit, not an immediate checkout discount. Free-delivery and BOGO savings stay unknown without basket information. All monetary fields currently use INR.

## Implemented flows

| Area | Behavior |
| --- | --- |
| Accounts | Registration, sign-in, revocable 7-day opaque bearer sessions, sign-out, preferred categories |
| Catalog | Existing brand/coupon endpoints, private creates, owner updates/deletes, admin publication |
| Discovery | Paginated text/category search, deterministic and opt-in AI recommendations |
| Imports | Manual fields, pasted email/notification text, screenshot OCR, AI draft extraction and review |
| Wallet | Save/unsave, on-device Room cache for saved offers, copy and self-reported use tracking |
| Reminders | Saved coupons expiring within 3 days; opt-in Android WorkManager checks roughly every 12 hours |
| Delivery | Docker, migrations, CI backend tests, Android lint/build, debug APK artifact |

## Project layout

- `backend/CouponHub.Domain`: original entities, value objects and coupon policy.
- `backend/CouponHub.Application`: original CQRS/repositories plus recommendation contracts and pure ranking rules.
- `backend/CouponHub.Infrastructure`: EF Core, account persistence, recommendation orchestration and OpenAI adapter.
- `backend/CouponHub.Api`: authenticated HTTP surface, access policies and composition root.
- `backend/CouponHub.Tests`: domain/ranking, AI contract and PostgreSQL integration tests.
- `android/app`: Compose screens, ViewModel, Retrofit, Keystore-protected session, Room cache, ML Kit OCR, WorkManager.

The Android app uses explicit dependency construction in `CouponHubApp`; the backend uses a small HTTP provider adapter rather than introducing Semantic Kernel for a single model call. The existing backend structure and PostgreSQL provider are retained.

## Validation

```bash
dotnet test backend/CouponHub.sln
# Integration tests additionally require a dedicated disposable database:
export COUPONHUB_TEST_DB='Host=localhost;Database=couponhub_tests;Username=couponhub;Password=YOUR_TEST_PASSWORD'
dotnet test backend/CouponHub.sln
gradle -p android :app:assembleDebug :app:lintDebug :app:testDebugUnitTest
```

CI provisions PostgreSQL, builds the solution, runs tests (including migration/model consistency and account-isolation checks), builds/lints Android and uploads a debug APK. Integration tests explicitly skip when `COUPONHUB_TEST_DB` is absent. They must not target a production database.

See [delivery status](docs/DELIVERY_STATUS.md) for what has actually been verified, [API guide](docs/API.md) for routes and [architecture notes](docs/ARCHITECTURE.md) for limits and data handling.
