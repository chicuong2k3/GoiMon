# Strawberry Shake — Blazor client GraphQL client

Summary
- The Blazor client for GoiMon will use Strawberry Shake (v15+). Reference: https://chillicream.com/docs/strawberryshake/v15/get-started

Why
- Generates a strongly-typed, compile-time GraphQL client for Blazor components.
- Integrates well with HotChocolate servers and supports DI registration, caching, and code generation workflows.

Quick notes / Next steps
1. Target project: GoiMon.Client
2. Add the Strawberry Shake tool / packages and configure code generation based on the server schema. See the Get Started guide above for exact commands.
3. Generate the client and commit the generated artifacts to the repository (or include generation as a build step in CI).
4. Register the generated client in `GoiMon.Client` DI container (Program.cs) and consume it from components/pages.

References
- Getting started: https://chillicream.com/docs/strawberryshake/v15/get-started
- API & configuration: https://chillicream.com/docs/strawberryshake/v15

Notes
- If you want, I can add a sample generator configuration file and the precise `dotnet` commands for GoiMon.Client next.