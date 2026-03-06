# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

GoiMon is a restaurant/food-shop management app (menu, staff, orders, revenue). It has two deployable projects:

- **GoiMon.Api** — ASP.NET Minimal API backend with a HotChocolate GraphQL endpoint, targeting .NET 10
- **GoiMon.Staff** — Blazor WebAssembly PWA frontend for staff, targeting .NET 10

The solution also uses the **BMAD Method** (`_bmad/` and `_bmad-output/`) for project management artifacts. Planning docs, architecture decisions, and dev stories live in `_bmad-output/`.

## Build / Run / Test Commands

```bash
# Restore all projects
dotnet restore GoiMon.sln

# Build the entire solution
dotnet build GoiMon.sln -c Release --no-restore

# Run the API locally (port 5000)
ASPNETCORE_URLS=http://localhost:5000 dotnet run --project src/GoiMon.Api -c Debug

# Run all tests
dotnet test tests/GoiMon.Api.Tests/GoiMon.Api.Tests.csproj -c Release --verbosity normal

# Run a single test by name
dotnet test tests/GoiMon.Api.Tests/GoiMon.Api.Tests.csproj --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Health check
curl http://localhost:5000/health
```

### EF Core Migrations

```bash
# Install the EF tool (if not present)
dotnet tool install --global dotnet-ef

# Create a new migration (run from repo root)
dotnet ef migrations add <MigrationName> -p src/GoiMon.Api -s src/GoiMon.Api --output-dir Infrastructure/Data/Migrations

# Apply migrations
dotnet ef database update -p src/GoiMon.Api -s src/GoiMon.Api
```

Migrations are auto-applied at API startup via `Database.Migrate()` + seed data.

### Strawberry Shake GraphQL Client (Staff frontend)

GraphQL operations are defined in `src/GoiMon.Staff/GraphQL/**/*.graphql`. The StrawberryShake code generator produces typed C# clients from these files. After editing `.graphql` files, rebuild to regenerate.

## Architecture

### GoiMon.Api (Backend)

The API uses a **feature-slice** layout with lightweight DDD conventions. There are no repository abstractions — HotChocolate resolvers query EF Core directly via a pooled `AppDbContext`.

**Key layers:**

- `Domain/` — Aggregate roots (`AggregateRoot` base class with domain events), entities, enums, and event definitions. Domain logic lives on the entity methods (e.g. `Order.AddItem()`, `Product.ChangePrice()`).
- `Features/` — Organized by feature (Products, Categories, Orders, Combos, Employees, Tables, Authentication, ImageUpload). Each feature contains its own Queries, Mutations, Validators, DTOs, and resolvers as HotChocolate type extensions.
- `Infrastructure/` — Cross-cutting: EF Core (`AppDbContext`, configurations, migrations, seed), Authorization policies, Outbox pattern (Hangfire-backed), DomainEvent dispatching, JWT/OTP services, FluentValidation middleware, Serilog, Cloudinary image upload, correlation-id middleware.

**GraphQL:** Implementation-first (no SDL files). The schema is composed by registering HotChocolate type extensions in `Program.cs`. Queries, mutations, and subscriptions are split across feature files. Filtering, sorting, and projections are enabled via HotChocolate.Data.

**Validation:** FluentValidation validators per feature, auto-applied via a custom HotChocolate field middleware (`FluentValidationMiddleware`). Validators are registered from the assembly at startup.

**Domain Events & Outbox:** Aggregate roots collect domain events. `AppDbContext.SaveChangesAsync` serializes them into an `OutboxEvents` table atomically. A Hangfire recurring job (`OutboxService`) processes pending events.

**Auth:** JWT Bearer authentication with role-based authorization policies defined in `Infrastructure/Authorization/`. User roles: Cashier, Supervisor, Manager, Owner, Accountant.

**Database:** PostgreSQL via Npgsql + EF Core. Connection string in `appsettings.json` (`ConnectionStrings:DefaultConnection`). Entity configurations use `IEntityTypeConfiguration<T>` in `Infrastructure/Data/Configurations/`.

### GoiMon.Staff (Frontend)

Blazor WebAssembly PWA using:
- **BlazorBlueprint** components for all UI (never raw HTML/Bootstrap)
- **StrawberryShake** for typed GraphQL client (`GoiMonStaff` client)
- **EasyAppDev.Blazor.Store** for state management with domain-scoped stores
- **Blazored.LocalStorage** for local persistence
- Tailwind CSS utility classes for layout

### CI/CD

GitHub Actions workflows deploy on push to `main`:
- `deploy-api.yml` — build, test, publish, SCP to server, restart systemd service
- `deploy-client.yml` — publish Blazor WASM, SCP wwwroot, deploy nginx config

## Conventions

- **Feature-based folder structure**: each feature is self-contained under `Features/{FeatureName}/`. Domain entities in `Domain/Entities/`, shared infra in `Infrastructure/`.
- **Global usings**: common namespaces are in `GlobalUsings.cs` per project. Prefer adding shared namespaces there rather than per-file.
- **GraphQL type extensions**: new queries/mutations/subscriptions must be registered as `AddTypeExtension<>()` in `Program.cs`.
- **Validators**: create FluentValidation `AbstractValidator<T>` classes in `Features/{Feature}/Validators/`. They are auto-discovered from the assembly.
- **Domain entities**: inherit from `AggregateRoot` for roots. Use encapsulated domain methods with domain events rather than anemic setters.
- **Order items use point-in-time snapshots**: `OrderItem` stores product name/price at order time, not FK references for display.
- **Test framework**: xUnit + Moq. Test project references the API project directly.
- **GraphQL endpoint**: `http://localhost:5000/graphql`. Use Banana Cake Pop or Nitro for schema exploration.
