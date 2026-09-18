# Vehicle Profit Tracker — API

ASP.NET Core Web API, rebuilt on the same layered architecture as the
`Startup-full` reference project: thin controllers → services → EF Core →
PostgreSQL, with global exception handling, JWT access+refresh tokens, and
rate-limited auth endpoints.

## Architecture

```
Controllers/        Thin HTTP layer — validates nothing itself, just calls a service
Services/           All business logic. One service per feature area.
Data/
  AppDbContext.cs
  Configurations/   One IEntityTypeConfiguration<T> per entity (Fluent API)
Models/             BaseEntity (Guid Id + CreatedAt/UpdatedAt) + domain entities
DTOs/                Request/response records — never expose entities directly
Exceptions/          Custom exceptions mapped to HTTP status codes
Middleware/          Global exception handler → consistent JSON error shape
```

**Why this shape:** controllers stay dumb and testable, services hold all the
rules (and can be unit-tested without spinning up HTTP), and validation
failures/not-found/conflict errors are thrown as typed exceptions rather than
checked with `if` blocks in every controller — `ExceptionHandlingMiddleware`
catches them once, in one place, and returns the same JSON shape everywhere:

```json
{ "status": 400, "message": "...", "errors": { "amount": ["..."] }, "traceId": "..." }
```

## What's included

- **Auth** (`AuthService`): register, login (issues an access token + refresh
  token pair), refresh (rotates the refresh token and detects reuse — if a
  already-used refresh token is replayed, every active session for that user
  is revoked as a precaution), logout, forgot/reset password.
- **Vehicles** (`VehicleService`): CRUD, always scoped to the authenticated
  owner via `ICurrentUserService`.
- **Transactions** (`TransactionService`): CRUD with filters (vehicle, date
  range, category), scoped through the vehicle's owner.
- **Dashboard** (`DashboardService`): `/today` and `/month` — profit is always
  `Income − Expense`, computed here in C#, never trusted from the client.
- **Security**: BCrypt password hashing, JWT bearer auth, per-endpoint rate
  limiting on the auth routes, baseline security headers, HTTPS redirection.
- **Ops**: `/health` endpoint, EF Core migrations auto-applied on startup
  (wrapped in try/catch so the app still boots if the DB is briefly
  unreachable), Dockerfile + docker-compose for local Postgres.

## 1. Configure secrets (don't edit appsettings.json directly)

```bash
cd API
cp appsettings.Local.json.example appsettings.Local.json
```

Edit `appsettings.Local.json` with your real Supabase connection string and a
long random JWT secret. This file is git-ignored — it's loaded last by
`Program.cs` and overrides the placeholders in `appsettings.json`.

## 2. Restore, migrate, run

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add Initial
dotnet run
```

Swagger opens automatically at `/swagger` (see `launchSettings.json`). Click
**Authorize** and paste `Bearer {token}` after logging in to test protected
routes.

Alternatively, run everything (API + local Postgres) with Docker:

```bash
docker compose up --build
```

## 3. Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/v1/auth/register` | No | `{ name, phone, password }` |
| POST | `/api/v1/auth/login` | No | `{ phone, password, rememberMe }` → access + refresh token |
| POST | `/api/v1/auth/refresh` | No | `{ refreshToken }` → new token pair (rotated) |
| POST | `/api/v1/auth/logout` | No | `{ refreshToken }` → revokes it |
| POST | `/api/v1/auth/forgot-password` | No | `{ phone }` → logs a reset code (swap for SMS later) |
| POST | `/api/v1/auth/reset-password` | No | `{ phone, token, newPassword }` |
| GET / POST | `/api/v1/vehicles` | Yes | List / add vehicles |
| PUT / DELETE | `/api/v1/vehicles/{id}` | Yes | Update / remove a vehicle |
| GET / POST | `/api/v1/transactions?vehicleId=&from=&to=&category=` | Yes | List (filtered) / add a transaction |
| PUT / DELETE | `/api/v1/transactions/{id}` | Yes | Update / remove a transaction |
| GET | `/api/v1/dashboard/today` | Yes | Today's income/expense/profit, per vehicle + total |
| GET | `/api/v1/dashboard/month` | Yes | Current month's summary |
| GET | `/health` | No | DB connectivity check |

## 4. Deploy

Set `ConnectionStrings__DefaultConnection` and `Jwt__Secret` as environment
variables on your host (Render/Railway/Azure/etc.) — same names as
`appsettings.Local.json`, just double-underscored, matching ASP.NET Core's
environment-variable configuration convention. Migrations apply automatically
on first boot.
