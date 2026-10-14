# Workspace Instructions

- The solution is `PortfolioTracker.sln`, targeting .NET 10.
- `Portfolio.Api` owns the controller API, EF Core entities, and `PortfolioDbContext`.
- `Portfolio.Client` is a standalone Blazor WebAssembly application. Keep its DTOs and form validation aligned with the API contracts.
- Route portfolio API calls through the scoped `IPortfolioApiService` in `Portfolio.Client/Services`.
- SQLite schema is initialized with EF Core `EnsureCreated`; update this setup deliberately if adding migrations.
- Keep CORS origins aligned with the local client launch profiles and API base URL in `Portfolio.Client/wwwroot/appsettings.json`.
- Keep output-cache tags and invalidation aligned when changing controller read or write actions.
- Validate changes with `dotnet build PortfolioTracker.sln`.
