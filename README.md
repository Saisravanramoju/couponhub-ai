# CouponHub AI

CouponHub AI is an Android coupon wallet backed by ASP.NET Core 9 and PostgreSQL. It supports private coupon imports, searchable public offers, saved coupons, expiry reminders, on-device screenshot OCR, optional OpenAI-assisted extraction, and personalized recommendations.

## What is included

| Area | Behavior |
| --- | --- |
| Accounts | Registration, sign-in, revocable seven-day bearer sessions, sign-out, and category preferences |
| Catalog | Brand and coupon APIs, private coupon creation, owner update/delete, and administrator publication |
| Discovery | Paginated text/category search and deterministic or opt-in AI recommendations |
| Imports | Manual entry, pasted text, screenshot OCR, optional AI extraction, and editable review |
| Wallet | Save/unsave, an on-device Room cache, coupon-code copy, and use tracking |
| Reminders | Optional WorkManager notifications for saved coupons expiring within three days |
| Delivery | Docker Compose, EF Core migrations, backend tests, Android build/lint/tests, and CI artifacts |

## Prerequisites

For the backend:

- Git
- Docker Desktop, or Docker Engine with Docker Compose v2
- Optional: .NET SDK 9 for running backend tests outside Docker

For Android development:

- Android Studio
- Android SDK Platform 35 and Android SDK Build-Tools 35.0.0
- JDK 17 or Android Studio's bundled JDK
- An Android emulator or a USB-debuggable device

The Gradle wrapper is committed to the repository. Do not install a separate Gradle version.

## Clone and configure

```bash
git clone https://github.com/Saisravanramoju/couponhub-ai.git
cd couponhub-ai
cp .env.example .env
```

On Windows PowerShell, use `Copy-Item .env.example .env`.

Open `.env` and set a long local database password. Use letters, numbers, hyphens, or underscores and do not use semicolons. OpenAI configuration is optional:

```dotenv
POSTGRES_PASSWORD=replace_with_a_long_local_password
OPENAI_API_KEY=
OPENAI_MODEL=gpt-4o-mini
```

Never commit `.env` or a real API key.

## Run the backend locally

From the repository root:

```bash
docker compose config --quiet
docker compose up --build -d
docker compose ps -a
```

Expected state:

- `db` is `healthy`;
- `migrate` is `Exited (0)` because it is a successful one-shot migration job;
- `api` is `Up` on `127.0.0.1:8080`.

Verify the API:

```bash
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
```

Open Swagger at <http://localhost:8080/swagger/index.html>.

Useful diagnostics:

```bash
docker compose logs --tail=200 api
docker compose logs --tail=200 migrate
docker compose logs --tail=200 db
```

PostgreSQL is exposed only on the local machine at `127.0.0.1:5433`. Database tools such as pgAdmin can connect with:

- Host: `127.0.0.1`
- Port: `5433`
- Database: `couponhub`
- Username: `couponhub`
- Password: the value of `POSTGRES_PASSWORD` in `.env`

## Create the first administrator

Create the administrator before registering the same email through the app:

```bash
export COUPONHUB_ADMIN_EMAIL='your-admin@example.com'
read -r -s -p 'Admin password (12-128 characters): ' COUPONHUB_ADMIN_PASSWORD
echo
export COUPONHUB_ADMIN_PASSWORD
docker compose run --rm \
  -e COUPONHUB_ADMIN_EMAIL \
  -e COUPONHUB_ADMIN_PASSWORD \
  api --create-admin
unset COUPONHUB_ADMIN_PASSWORD
```

PowerShell:

```powershell
$env:COUPONHUB_ADMIN_EMAIL = 'your-admin@example.com'
$securePassword = Read-Host 'Admin password (12-128 characters)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
$env:COUPONHUB_ADMIN_PASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
docker compose run --rm -e COUPONHUB_ADMIN_EMAIL -e COUPONHUB_ADMIN_PASSWORD api --create-admin
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
Remove-Item Env:COUPONHUB_ADMIN_PASSWORD
```

No merchant coupons, administrator accounts, or default passwords are seeded. Register a separate regular account when testing private imports.

## Use Swagger authentication

1. Run `POST /api/account/register` or `POST /api/account/login`.
2. Copy the returned `accessToken`.
3. Select **Authorize** in Swagger.
4. Paste the token value. Swagger adds the `Bearer` scheme.
5. Call authenticated endpoints.

## Configure optional OpenAI features

Add the server-side key and a compatible Chat Completions model to `.env`, then recreate the API container:

```dotenv
OPENAI_API_KEY=your_key_here
OPENAI_MODEL=gpt-4o-mini
```

```bash
docker compose up -d --force-recreate api
docker compose exec api sh -lc 'test -n "$AI__ApiKey" && echo "AI key configured" || echo "AI key missing"; echo "AI model: $AI__Model"'
```

The key remains on the backend and is never embedded in the Android application.

- Screenshot OCR runs on the device with ML Kit.
- The user reviews the OCR text and must opt in before text is sent for AI extraction.
- AI output is treated as untrusted input and validated before it becomes a coupon draft.
- Recommendation candidates are filtered by ownership, state, expiry, brand, and minimum order before optional AI ranking.
- Only IDs from the server-generated shortlist are accepted from the model.
- Without an API key, accounts, manual imports, search, saved coupons, and deterministic recommendations still work. AI extraction returns HTTP 503 so the app can offer manual entry.

## Build and run Android

Keep the backend running. The Android emulator reaches the host API through `10.0.2.2`.

macOS or Linux:

```bash
cd android
chmod +x gradlew
./gradlew :app:assembleDebug
./gradlew :app:lintDebug
./gradlew :app:testDebugUnitTest
./gradlew :app:installDebug
```

Windows PowerShell:

```powershell
cd android
.\gradlew.bat :app:assembleDebug
.\gradlew.bat :app:lintDebug
.\gradlew.bat :app:testDebugUnitTest
.\gradlew.bat :app:installDebug
```

The debug APK is generated at `android/app/build/outputs/apk/debug/app-debug.apk`.

If Terminal cannot find Java on macOS, use Android Studio's bundled runtime for the current shell:

```bash
export JAVA_HOME="/Applications/Android Studio.app/Contents/jbr/Contents/Home"
export PATH="$JAVA_HOME/bin:$PATH"
java -version
```

Confirm that an emulator or device is connected:

```bash
adb devices
```

For a USB-connected physical device, forward the API port and build against device-local `localhost`:

```bash
adb reverse tcp:8080 tcp:8080
./gradlew :app:installDebug -PAPI_BASE_URL=http://localhost:8080/
```

For a release or another environment, provide an HTTPS URL ending in `/`:

```bash
./gradlew :app:assembleRelease -PAPI_BASE_URL=https://api.example.com/
```

Release builds reject cleartext HTTP. The supplied Compose configuration binds services to loopback and is intended only for local development.

## Run automated tests

Backend unit tests do not require PostgreSQL. Integration tests skip when `COUPONHUB_TEST_DB` is absent:

```bash
dotnet restore backend/CouponHub.sln
dotnet build backend/CouponHub.sln -c Release --no-restore
dotnet test backend/CouponHub.sln -c Release --no-build
```

To run all backend tests against a disposable PostgreSQL database:

```bash
docker run --rm -d \
  --name couponhub-test-db \
  -e POSTGRES_USER=couponhub \
  -e POSTGRES_PASSWORD=ci-only-password \
  -e POSTGRES_DB=couponhub_tests \
  -p 127.0.0.1:55432:5432 \
  postgres:17

export COUPONHUB_TEST_DB='Host=127.0.0.1;Port=55432;Database=couponhub_tests;Username=couponhub;Password=ci-only-password'
export ConnectionStrings__DefaultConnection="$COUPONHUB_TEST_DB"
dotnet test backend/CouponHub.sln -c Release
docker stop couponhub-test-db
```

Do not point integration tests at a shared or production database.

Android verification:

```bash
./android/gradlew -p android \
  :app:assembleDebug \
  :app:lintDebug \
  :app:testDebugUnitTest \
  --no-daemon
```

CI provisions PostgreSQL, runs all backend tests, installs Android SDK 35, executes the committed Gradle wrapper, builds and lints Android, runs Android unit tests, and uploads the debug APK.

## Stop or reset local services

Stop containers without deleting database data:

```bash
docker compose down
```

Delete containers and the local PostgreSQL volume only when you intentionally want to erase local data:

```bash
docker compose down -v
```

## Troubleshooting

### Docker reports no configuration file

Run Compose from the cloned repository root, where `compose.yaml` is located.

### Compose reports a YAML parser error

Validate indentation with:

```bash
docker compose config --quiet
```

YAML list entries under `ports`, `environment`, and `volumes` must remain indented beneath their parent key.

### PostgreSQL rejects the configured password

Changing `POSTGRES_PASSWORD` does not modify a password stored in an existing volume. For disposable local data, run `docker compose down -v` and start again. Otherwise, change the database role password to match `.env` without deleting the volume.

### The API returns 401

Log in again and replace the bearer token. Sessions expire after seven days and are revoked by logout.

### AI extraction returns 503

Confirm that `OPENAI_API_KEY` and `OPENAI_MODEL` are present in `.env`, recreate the API container, and review `docker compose logs api`. Also confirm API billing/model access for the key's project.

### Android cannot reach the API

- Start the API and verify `http://localhost:8080/health/ready` on the computer.
- Use `http://10.0.2.2:8080/` for an emulator.
- Use `adb reverse` and `http://localhost:8080/` for a USB-connected device.
- Do not use the computer's `localhost` from an emulator; it points to the emulator itself.

### Android SDK XML version warning

Update Android Studio's SDK Command-line Tools from **SDK Manager → SDK Tools** so the IDE and command-line packages are from compatible releases.

## Production notes

The Compose setup runs the API in Development and is not an internet-facing deployment configuration. Production deployment requires HTTPS, `ASPNETCORE_ENVIRONMENT=Production`, managed secrets, persistent data-protection keys, database backups, monitoring, rate-limit review, and a configured host name. Configure trusted forwarded headers only for the specific reverse proxy in front of the API.

## Project layout

- `backend/CouponHub.Domain`: entities, value objects, and coupon policy.
- `backend/CouponHub.Application`: CQRS/repositories, recommendation contracts, and ranking rules.
- `backend/CouponHub.Infrastructure`: EF Core, account persistence, recommendation orchestration, and the OpenAI adapter.
- `backend/CouponHub.Api`: authenticated HTTP endpoints, access policies, middleware, and composition root.
- `backend/CouponHub.Tests`: domain, ranking, AI contract, migration, and PostgreSQL integration tests.
- `android/app`: Compose UI, ViewModel, Retrofit, encrypted session storage, Room cache, ML Kit OCR, and WorkManager.

See [delivery status](docs/DELIVERY_STATUS.md), [API guide](docs/API.md), and [architecture notes](docs/ARCHITECTURE.md).
