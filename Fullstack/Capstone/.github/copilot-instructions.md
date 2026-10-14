# Workspace Instructions

- The solution is `PortfolioTracker.sln`, targeting .NET 10.
- `Portfolio.Api` owns the controller API, EF Core entities, and `PortfolioDbContext`.
- `Portfolio.Client` is a standalone Blazor WebAssembly application. Keep its DTOs and form validation aligned with the API contracts.
- Route portfolio API calls through the scoped `IPortfolioApiService` in `Portfolio.Client/Services`.
- Keep logged-in user state in the scoped `IAuthStateService` in `Portfolio.Client/Services`.
- SQLite schema is managed with EF Core migrations; the startup initializer bridges databases created by the original `EnsureCreated` setup.
- Keep Identity account ownership filters and `[Authorize]` requirements aligned across portfolio and project controller actions.
- Keep CORS origins aligned with the local client launch profiles and API base URL in `Portfolio.Client/wwwroot/appsettings.json`.
- Keep output-cache tags and invalidation aligned when changing controller read or write actions.
- Validate changes with `dotnet build PortfolioTracker.sln`.
