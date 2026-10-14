# Workspace Instructions

- The solution is `PortfolioTracker.sln`, targeting .NET 10.
- `Portfolio.Api` owns the minimal API, EF Core entities, and `PortfolioDbContext`.
- `Portfolio.Client` is a standalone Blazor WebAssembly application. Keep its DTOs and form validation aligned with the API contracts.
- SQLite schema is initialized with EF Core `EnsureCreated`; update this setup deliberately if adding migrations.
- Keep CORS origins aligned with the local client launch profiles and API base URL in `Portfolio.Client/wwwroot/appsettings.json`.
- Validate changes with `dotnet build PortfolioTracker.sln`.
