# Authorization Configuration

## Overview

The Conferences API uses JWT-based authentication with Auth0 and implements a permission-based authorization system.

## Authentication

All endpoints require authentication by default unless explicitly marked with `AllowAnonymous()`.

**Configuration:**
- **Authority**: `https://attendr.eu.auth0.com/`
- **Audience**: `https://api.attendr.com`
- **Scheme**: JWT Bearer

## Authorization Policies

### 1. Default Policy (Fallback)
All endpoints require an authenticated user by default. No explicit `RequireAuthorization()` call is needed.

### 2. Authenticated Policy
Explicitly requires the user to be authenticated. This is the same as the default policy but can be used for clarity.

**Usage:**
```csharp
.RequireAuthorization(AuthorizationPolicies.Authenticated)
```

### 3. Admin Policy
Requires the user to have the `admin:attendr` permission in their JWT token.

**Usage:**
```csharp
.RequireAuthorization(AuthorizationPolicies.Admin)
```

## Permissions

Permissions are expected to be in the JWT token claims under the `permissions` claim. Auth0 typically sends permissions as a space-separated string.

### Available Permissions

- **`admin:attendr`**: Grants administrative access to the Attendr application

## Endpoint Authorization

### Public Endpoints (AllowAnonymous)
- Integration endpoints (`/api/conferences-integration/*`)
- Event handler endpoints (`/api/EventHandlers/*`)

### Authenticated Endpoints (Default Policy)
- `GET /api/conferences` - List conferences
- `GET /api/conferences/{id}` - Get conference details
- `POST /api/conferences/{id}/follow` - Follow a conference

### Admin-Only Endpoints
- `POST /api/conferences` - Create conference
- `PUT /api/conferences/{id}` - Update conference

## Permission Handler

The `PermissionAuthorizationHandler` validates permissions from JWT tokens:

1. Checks for a `permissions` claim containing space-separated permissions
2. Verifies the required permission is present
3. Also supports individual permission claims as an alternative format

## Examples

### JWT Token Structure
```json
{
  "permissions": "admin:attendr read:conferences write:conferences",
  "sub": "auth0|123456",
  "aud": "https://api.attendr.com"
}
```

### Creating a New Admin-Protected Endpoint
```csharp
group.MapPost("/admin-action", AdminAction)
    .WithName("AdminAction")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .RequireAuthorization(AuthorizationPolicies.Admin);
```

### Creating a Public Endpoint
```csharp
group.MapGet("/public-data", PublicData)
    .WithName("PublicData")
    .Produces(StatusCodes.Status200OK)
    .AllowAnonymous();
```

## Error Responses

- **401 Unauthorized**: User is not authenticated
- **403 Forbidden**: User is authenticated but lacks required permissions

## Adding New Permissions

1. Add the permission constant to `Authorization/Permissions.cs`
2. Create a new policy in `Program.cs` using `PermissionRequirement`
3. Apply the policy to endpoints using `RequireAuthorization()`

Example:
```csharp
// In Permissions.cs
public const string ManageUsers = "manage:users";

// In Program.cs
.AddPolicy("ManageUsers", policy =>
    policy.Requirements.Add(new PermissionRequirement(Permissions.ManageUsers)))

// In endpoint configuration
.RequireAuthorization("ManageUsers")
```
