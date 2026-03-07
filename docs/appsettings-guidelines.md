**Appsettings guidelines — Development / Production**

Goal: keep secrets out of git, provide safe committed templates, and make it clear how to run locally and in production.

Files and intended usage
- `src/GoiMon.Api/appsettings.json` — committed template with placeholders (non-sensitive). This is the baseline configuration loaded for all environments.
- `src/GoiMon.Api/appsettings.Development.json` — local development settings (contains real dev secrets). This file is ignored by git and should live only on developer machines.
- (No staging file required) Use `appsettings.json` as the committed template and override sensitive values via environment variables or a secret store in CI/deploy.
- Production secrets should never be committed. Use environment variables or a secret manager (Azure Key Vault, AWS Secrets Manager, etc.).

How ASP.NET Core chooses configuration
- The runtime loads `appsettings.json` then `appsettings.{Environment}.json` where `Environment` is set by `ASPNETCORE_ENVIRONMENT` (e.g., Development, Staging, Production). Environment-specific files override base keys.

Local development (quick start)

1. Keep `ASPNETCORE_ENVIRONMENT=Development` for both your local developer runs and the "dev" deployed environment used by QA/testers. Do not commit secrets to the repo — use one of the approaches below to provide secrets locally.

2. Options for providing secrets locally (choose one):

- Use `appsettings.Development.json` on your machine (git-ignored) and put developer secrets there.

- Use `dotnet user-secrets` (recommended):

```bash
cd src/GoiMon.Api
dotnet user-secrets init
dotnet user-secrets set "Cloudinary:CloudName" "dz4xapwfd"
dotnet user-secrets set "Cloudinary:ApiKey" "..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/GoiMon.Api
```

- Or export environment variables directly (shell):

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=goimon_dev;Username=postgres;Password=postgres;"
export Cloudinary__CloudName="dz4xapwfd"
export Cloudinary__ApiKey="..."
export Cloudinary__ApiSecret="..."
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/GoiMon.Api
```

Production deployment (recommended approach)
- Commit `appsettings.json` as a non-sensitive template. In your production deployment pipeline, inject real secrets using environment variables or the platform's secret storage.
- Example: in your CI/CD, set env vars before starting the app. The app will read the env vars which override values in `appsettings.json`.

Useful env var naming convention
- Use double-underscore `__` to represent nested keys in environment variables. Example:
  - `ConnectionStrings__DefaultConnection`
  - `OAuth__Google__ClientSecret`
  - `Jwt__SigningKey`

Dotnet EF / migrations with environment
- To run migrations against the development DB:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/GoiMon.Api
```

Security recommendations
- Never commit files that contain production credentials. Use the OS/env or a secrets manager.
- For local dev you can use `dotnet user-secrets` for per-developer secrets (only for Development environment):

```bash
cd src/GoiMon.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "dev-secret"
```

If you want, I can:
- Add a `appsettings.Development.json.template` (safe template) file so developers can copy it.
- Add CI/CD examples for GitHub Actions showing how to inject production env vars.

**Required GitHub Secrets**

Recommended secret names to create in the repository (only add real secret values here):

- Dev deployment (used by workflows when pushing to `dev` branch):
  - `SSH_HOST_DEV` — SSH hostname/IP for dev server
  - `SSH_USER_DEV` — SSH username for dev server
  - `SSH_PRIVATE_KEY_DEV` — private key used for SCP/SSH to dev server

- Production deployment (used by workflows when pushing to `main` branch):
  - `SSH_HOST` — SSH hostname/IP for production server
  - `SSH_USER` — SSH username for production server
  - `SSH_PRIVATE_KEY` — private key used for SCP/SSH to production server

- Optional runtime secrets (only if you want CI to inject them; otherwise set them on the target host/service):
  - `DEFAULT_CONNECTION` — full DB connection string for CI migration step
  - `JWT_SIGNING_KEY` — JWT signing key
  - `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET` — Cloudinary credentials
  - `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET` — Google OAuth credentials
  - `FACEBOOK_APP_ID`, `FACEBOOK_APP_SECRET` — Facebook OAuth credentials

How to add via GitHub web UI:

1. Go to your repository → Settings → Secrets and variables → Actions → New repository secret.
2. Enter the secret name (from the list above) and its value, then Save.

Quick `gh` CLI example:

```bash
gh secret set SSH_HOST --body "1.2.3.4"
gh secret set SSH_USER --body "deploy"
gh secret set SSH_PRIVATE_KEY --body "$(cat ~/.ssh/id_rsa)"
```

Security notes:
- Only store real secrets in GitHub Secrets or a secrets manager. Do not commit them into any `appsettings.*.json` file.
- Prefer configuring runtime secrets on the target host or using a platform secret integration (Azure Key Vault, AWS Secrets Manager) where possible.
