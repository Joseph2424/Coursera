# Portfolio Tracker

A local portfolio and project tracking application with a Blazor WebAssembly client, ASP.NET Core controller API, and SQLite database using EF Core.

## Requirements

- .NET 10 SDK

## Run locally

Start the API in one terminal:

```sh
dotnet run --project Portfolio.Api --launch-profile https
```

Start the client in a second terminal:

```sh
dotnet run --project Portfolio.Client --launch-profile https
```

Open `https://localhost:7261`. The client calls the API at `https://localhost:7021`. If the browser does not trust the local HTTPS certificate, trust the .NET development certificate for this machine and restart both processes.

The API applies EF Core migrations on startup. Existing databases created by the earlier `EnsureCreated` setup are recognized and upgraded without dropping portfolio tables; back up `Portfolio.Api/portfolio.db` before upgrading. Existing profiles keep their data but remain unowned and hidden until assigned to an account.

Development uses a randomly generated, process-local JWT signing key, so restarting the API invalidates existing access tokens. Before running outside Development, set `Jwt__Key` through a secret manager or environment variable to a random secret of at least 32 bytes. `Jwt__Issuer`, `Jwt__Audience`, and `Jwt__LifetimeMinutes` can also be configured.

## Features

- Create and update portfolio profiles with name, email, bio, and comma-separated skills.
- View profiles and skills belonging to the signed-in account.
- Create, list, update, and delete projects for the selected profile.
- Validate profile and project input in the client and API.
- Store profiles, projects, and reusable skills in SQLite.
- Cache successful GET responses in memory for 30 seconds; writes evict affected cache tags.
- Call the API through the scoped `IPortfolioApiService` registered in the Blazor client.
- Keep the signed-in account in the scoped `IAuthStateService`; session restoration and login/logout changes notify subscribed components.
- Register and log in with ASP.NET Core Identity; the client keeps the bearer token in session storage.

## API

- `POST /api/identity/register` (anonymous)
- `POST /api/identity/login` (anonymous)
- `GET /api/identity/me` (bearer token required)
- `GET /api/portfolio-users`
- `GET /api/portfolio-users/{id}`
- `POST /api/portfolio-users`
- `PUT /api/portfolio-users/{id}`
- `GET /api/projects?portfolioUserId={id}`
- `POST /api/projects?portfolioUserId={id}`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`
- Development OpenAPI document: `/openapi/v1.json`

All portfolio and project routes require a valid bearer token and are scoped to its account. Registration enforces a 10-character minimum with uppercase, lowercase, and numeric characters. Login failures are locked out after five attempts for 15 minutes. Do not expose the development signing key or use Development settings in production.

The EF Core model uses a one-to-many relationship from portfolio users to projects and a many-to-many relationship between users and skills. Profile emails and skill names are unique case-insensitively.

Controller GET responses use ASP.NET Core output caching, keyed by authenticated account. Project responses also vary by `portfolioUserId`; profile or project changes evict affected cached responses. The cache is process-local and is cleared when the API restarts.

## Build

```sh
dotnet build PortfolioTracker.sln
```

## End-to-end tests

The NUnit Playwright test starts the API and Blazor client on their development HTTPS ports and uses a temporary SQLite database. Stop any manually running app processes before running it.

```sh
dotnet build PortfolioTracker.sln
./e2e/Portfolio.E2E.Tests/bin/Debug/net10.0/.playwright/node/darwin-arm64/node ./e2e/Portfolio.E2E.Tests/bin/Debug/net10.0/.playwright/package/cli.js install chromium
dotnet test PortfolioTracker.sln --no-build
```

On Windows or Linux, install Chromium with `pwsh e2e/Portfolio.E2E.Tests/bin/Debug/net10.0/playwright.ps1 install chromium` instead. The same suite runs in GitHub Actions on pushes, pull requests, and manual dispatch. It covers registration/login, profile and skill updates, project create/update/delete, anonymous 401 responses, and cross-account 404 responses.