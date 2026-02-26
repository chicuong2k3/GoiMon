EF Migrations & Seeding

1. Create a new migration (requires dotnet-ef tool):

```bash
dotnet tool install --global dotnet-ef
cd src/GoiMon.Api
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Data/Migrations
```

2. Apply migrations (from project folder):

```bash
dotnet ef database update --context AppDbContext
```

3. The application will also attempt to run `Database.Migrate()` and seed sample products at startup.

Notes:
- Ensure `appsettings.json` connection string is correct and reachable.
- Use `DesignTimeDbContextFactory` if `dotnet ef` needs context creation assistance.
