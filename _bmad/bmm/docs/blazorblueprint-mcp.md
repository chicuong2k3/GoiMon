# Blazor Blueprint MCP (Model Context Protocol)

This repository includes a project-level MCP config to expose the Blazor Blueprint MCP server to AI coding assistants.

Files
- `.mcp.json` — registers the `blazorblueprint` MCP server using `npx -y @blazorblueprint/mcp`.

Run the MCP server locally (requires Node.js 18+):

```bash
npx @blazorblueprint/mcp@latest
```

Add to Claude Code (example):

macOS / Linux

```bash
claude mcp add blazorblueprint --transport stdio -- npx -y @blazorblueprint/mcp@latest
```

Windows

```powershell
claude mcp add blazorblueprint --transport stdio -- cmd /c npx -y @blazorblueprint/mcp@latest
```

Notes
- Pin a version via `BLAZORBLUEPRINT_VERSION` env var when adding the server to Claude Code.
- The MCP server provides fuzzy search, component docs, blueprints, and changelog tools.
