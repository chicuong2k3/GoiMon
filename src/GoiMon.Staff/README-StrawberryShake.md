# Strawberry Shake setup (local steps)

1. Create a tool manifest and install the Strawberry Shake CLI (one-time):

```bash
cd /home/chicuong/Desktop/code/GoiMon
dotnet new tool-manifest --force
dotnet tool install StrawberryShake.Tools --version 15.*
dotnet tool restore
```

2. Initialize a client against the local API (one-time scaffold):

```bash
cd /home/chicuong/Desktop/code/GoiMon
dotnet new tool-manifest --force
dotnet tool install StrawberryShake.Tools --version 15.*
dotnet tool restore
dotnet graphql init http://localhost:5000/graphql -n GoiMonStaff -p ./src/GoiMon.Staff
```

3. Or generate from the existing `strawberryshake.yml` config:

```bash
cd /home/chicuong/Desktop/code/GoiMon
dotnet tool restore
dotnet tool run strawberryshake generate --config strawberryshake.yml
```

After generation:
- Generated C# client will be in `src/GoiMon.Staff/Generated` (as configured).
- Ensure the `GoiMon.Api` server is running at `http://localhost:5000/graphql` before generating with `init`.

Notes:
- The tool name used above is `StrawberryShake.Tools` and the CLI command is available as `strawberryshake` via `dotnet tool run`.
- If you prefer a global install: `dotnet tool install --global StrawberryShake.Tools` (optional).
