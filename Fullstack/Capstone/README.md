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

The API creates `portfolio.db` on startup using EF Core `EnsureCreated`. The database is local development data; back it up before removing it. The API currently has no authentication and should not be exposed to the public internet.

## Features

- Create and update portfolio profiles with name, email, bio, and comma-separated skills.
- View multiple profiles and their skills.
- Create, list, and update projects for the selected profile.
- Validate profile and project input in the client and API.
- Store profiles, projects, and reusable skills in SQLite.
- Cache successful GET responses in memory for 30 seconds; writes evict affected cache tags.
- Call the API through the scoped `IPortfolioApiService` registered in the Blazor client.

## API

- `GET /api/portfolio-users`
- `GET /api/portfolio-users/{id}`
- `POST /api/portfolio-users`
- `PUT /api/portfolio-users/{id}`
- `GET /api/projects?portfolioUserId={id}`
- `POST /api/projects?portfolioUserId={id}`
- `PUT /api/projects/{id}`
- Development OpenAPI document: `/openapi/v1.json`

The EF Core model uses a one-to-many relationship from portfolio users to projects and a many-to-many relationship between users and skills. Profile emails and skill names are unique case-insensitively.

Controller GET responses use ASP.NET Core output caching. Project responses vary by `portfolioUserId`; profile or project changes evict affected cached responses. The cache is process-local and is cleared when the API restarts.

## Build

```sh
dotnet build PortfolioTracker.sln
```