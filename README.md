# Bromcom Reset — Graph PoC API

A secure, minimal ASP.NET Core (.NET 8) API to quickly reset student passwords via Microsoft Graph
(with `forceChangePasswordNextSignIn`) — designed as the engine behind a smoother Bromcom/MCAS reset flow.

> This PoC focuses on the Azure AD/Microsoft Graph route because it's the fastest to automate safely.
> You can add Bromcom Partner API calls later for MCAS reset emails once the school grants your app scopes.

## Features
- **POST `/api/reset/graph`** — set a temporary password for a user (by UPN or ID) and require change at next sign-in.
- **SSO protection (JWT)** using Entra ID (Azure AD) via `Microsoft.Identity.Web`.
- **Role gating via group** — restrict to a single Helpdesk/IT group by Group Object ID.
- **Audit logging** (append-only JSONL) to `logs/audit.log` by default.
- **Health endpoint** at `/healthz`.

## Quick start

### 1) Prereqs
- .NET 8 SDK
- An Entra ID (Azure AD) tenant with permission to create an app registration.
- The API will be protected by Entra ID; calls require a user token from your tenant.
- The same app registration is also used (with client credentials) to call Microsoft Graph for password resets.

### 2) App registration (Entra ID)
Run through these steps (or adapt the provided script in `scripts/create-azad-app.ps1`).

- Create an App Registration, e.g. **BromcomReset-API**.
- Expose an API (set Application ID URI or leave default).
- Add these **API permissions** (Application type):  
  - `Microsoft Graph > Application > User.ReadWrite.All` (admin consent)
- Create a **client secret** and note it.
- (Optional but recommended) Create a **security group** (e.g. *IT Helpdesk*) and capture its **Object ID**. Members of this group will be allowed to use the API.
- Under **Authentication**, add a web redirect URI if you plan to use auth code flow in a UI. For API-only JWT, none is needed.

> In production, prefer the least-privileged Graph role that allows writing `passwordProfile`; at time of writing `User.ReadWrite.All` is typically required.

### 3) Configure `appsettings.json`
Fill these values:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<YOUR_TENANT_ID>",
    "ClientId": "<YOUR_APP_CLIENT_ID>",
    "ClientSecret": "<YOUR_CLIENT_SECRET>", 
    "Audience": "<YOUR_API_AUDIENCE_OR_CLIENTID>"
  },
  "Reset": {
    "AllowedGroupObjectId": "<GROUP_OBJECT_ID_FOR_HELPDESK>"
  },
  "Audit": {
    "Path": "logs/audit.log"
  }
}
```

- `Audience` should match the API's expected audience (often the same as `ClientId` for simple bearer validation).

### 4) Run
```bash
dotnet restore
dotnet build
dotnet run --project src/Api/BromcomReset.Api.csproj
```

### 5) Test
- Get a user token from your tenant (e.g., Azure CLI or Postman with Entra auth) that includes this API as audience.
- Call:
```http
POST http://localhost:5080/api/reset/graph
Content-Type: application/json
Authorization: Bearer <user_jwt>

{
  "userPrincipalName": "student@school.org",
  "reason": "Forgot password at desk"
}
```
- Response returns a generated temp password (if not supplied), and audit entry is written.

### 6) Docker
```bash
docker build -t bromcom-reset-api:dev .
docker run -p 5080:8080 --env-file ./src/Api/.env bromcom-reset-api:dev
```

> You may prefer to pass sensitive config via environment variables instead of `appsettings.json` in production.

## Adding Bromcom Partner API (later)
Once your Partner Portal app is approved and the school grants scopes, add a `BromcomService` and a `/api/reset/mcas` action to trigger MCAS reset emails. Keep the same RBAC + auditing.

## Structure
```
/src/Api
  Controllers/
  Services/
  Models/
  Auth/
  Properties/
Dockerfile
docker-compose.yml
scripts/
```

## Notes
- This is a PoC; review security, rate limiting, error handling, and logging before production deployment.
- Do **not** email plaintext passwords. Prefer a printed slip or staff-verbal with immediate change enforced.
