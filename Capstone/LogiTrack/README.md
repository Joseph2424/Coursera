# LogiTrack

LogiTrack is an ASP.NET Core 10 Web API for inventory and customer-order management across fulfillment centers. It uses controller-based routes, EF Core with SQLite, ASP.NET Core Identity, JWT bearer authentication, and in-memory caching.

## Requirements

- .NET 10 SDK

## Run locally

Configure the bootstrap administrator before starting the API:

```sh
BootstrapAdmin__Email=admin@example.com \
BootstrapAdmin__Password='Choose-A-Strong-Password-123!' \
dotnet run --launch-profile http
```

The HTTP launch profile listens on `http://localhost:5225`. In Development, OpenAPI is available at `http://localhost:5225/openapi/v1.json`. Development generates an ephemeral signing key when `Authentication__SigningKey` is not set, so tokens stop working after an application restart.

See [AUTHENTICATION.md](AUTHENTICATION.md) for admin provisioning and signing-key configuration. The [LogiTrack.http](LogiTrack.http) file contains ready-to-run authentication and API requests.

## API routes

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Anonymous | Register a standard user |
| `POST` | `/api/auth/login` | Anonymous | Exchange credentials for a bearer token |
| `GET` | `/api/fulfillment-centers` | Authenticated | List centers |
| `GET` | `/api/fulfillment-centers/{id}` | Authenticated | Get a center |
| `POST` | `/api/fulfillment-centers` | Admin | Create a center |
| `GET` | `/api/inventory` | Authenticated | List inventory across centers |
| `GET` | `/api/inventory/{id}` | Authenticated | Get an inventory item |
| `POST` | `/api/inventory` | Admin | Add inventory |
| `PUT` | `/api/inventory/{id}` | Admin | Replace an inventory item |
| `GET` | `/api/orders` | Admin | List orders |
| `GET` | `/api/orders/{id}` | Admin | Get an order |
| `POST` | `/api/orders` | Admin | Create an order and reserve stock |
| `PUT` | `/api/orders/{id}` | Admin | Update order details or status |

Send protected requests with `Authorization: Bearer <access-token>`. New registrations always receive the `User` role; public registration cannot grant admin access. Order routes are admin-only because orders contain customer information.

## Data and persistence

The default configuration uses two local SQLite databases:

- `logitrack.db` stores fulfillment centers, inventory, orders, and order lines.
- `logitrack.identity.db` stores users, roles, and Identity data.

Both files persist across application restarts and are ignored by Git. The API creates schemas with EF Core `EnsureCreated` at startup. On an empty business database, startup also seeds three sample fulfillment centers, five inventory items, and three sample orders. This seeding currently runs in every environment when the center table is empty; disable or make it environment-specific before deploying to a production database. `EnsureCreated` does not evolve existing schemas; use reviewed EF Core migrations for schema changes in deployed environments.

Creating an order validates stock at its fulfillment center and reserves the requested quantities. Cancelling an order restores those quantities. Order line prices are captured when the order is created.

## Caching and queries

Fulfillment-center and inventory list/detail responses, plus the order list, are cached in-process for 30 seconds. Writes invalidate affected cached data. Order details are not cached because they include related inventory names and SKUs. The cache is local to one API process; use a distributed cache or another cross-instance invalidation strategy if running multiple API instances.

Read-only queries use `AsNoTracking`. Inventory and order reads eager-load the related fulfillment center and order-line inventory data needed to build responses.

## Build and tests

Build the API:

```sh
dotnet build
```

Run the full integration suite:

```sh
dotnet test tests/LogiTrack.IntegrationTests/LogiTrack.IntegrationTests.csproj
```

The tests use isolated SQLite files and cover inventory/order creation and stock reservation, Identity and role access, invalid input, cache hits/invalidation and request timings, and persistence across an application restart.

## Project layout

- `Controllers/` contains HTTP routes, request handling, and authorization attributes.
- `Contracts/` contains request and response DTOs with validation rules.
- `Services/` contains order/inventory business logic, EF Core queries, and cache behavior.
- `Models/` and `Data/` contain the relational entities, contexts, and database initialization.
- `Security/` contains shared role and token settings.
- `Middleware/` contains centralized exception-to-Problem-Details handling.
- `tests/LogiTrack.IntegrationTests/` contains end-to-end API tests.