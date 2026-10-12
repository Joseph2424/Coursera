# Authentication

The API uses ASP.NET Core Identity for accounts and stores identity data in `logitrack.identity.db`, separate from the existing inventory and order database.

## Bootstrap the first admin

Set bootstrap credentials before starting the app. The account is created in the `Admin` role on startup; public registration always creates users in the `User` role.

```sh
BootstrapAdmin__Email=admin@example.com \
BootstrapAdmin__Password='Choose-A-Strong-Password-123!' \
dotnet run
```

In Development, the app generates an ephemeral signing key if `Authentication__SigningKey` is unset. Tokens expire when the app restarts. Outside Development, configure a stable signing key of at least 32 bytes using a secrets manager or environment variable:

```sh
Authentication__SigningKey='your-stable-secret-of-at-least-32-bytes' dotnet run
```

Do not commit real passwords or signing keys.

## Endpoints and roles

- `POST /api/auth/register` creates a standard `User` account.
- `POST /api/auth/login` returns a bearer access token.
- Authenticated users can read fulfillment centers and inventory.
- Only `Admin` can create fulfillment centers or create/update inventory.
- All order endpoints require the `Admin` role because orders contain customer information.

Send the login token on protected requests as `Authorization: Bearer <access-token>`. The `LogiTrack.http` file contains example requests.