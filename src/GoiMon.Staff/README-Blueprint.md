# Blazor Blueprint — Integration Guide

This file shows how to enable Blazor Blueprint in the `GoiMon.Staff` project.

Quick (safe) setup — placeholder only
- A lightweight placeholder CSS and a sample component were added so you can preview Blueprint-like styles without installing any NuGet packages:
  - `wwwroot/lib/blazorblueprint/blazor-blueprint.css`
  - `Shared/BlueprintSample.razor`

Full integration (recommended)
1. Install the official NuGet package (example package id — replace with the exact package if it differs):

```bash
cd /home/chicuong/Desktop/code/GoiMon/src/GoiMon.Staff
dotnet add package BlazorBlueprintUI
```

2. Add any required CSS/JS bundles to `wwwroot/index.html` (the package docs will show exact files). Example:

```html
<link rel="stylesheet" href="_content/BlazorBlueprintUI/blazor-blueprint.min.css" />
<script src="_content/BlazorBlueprintUI/blazor-blueprint.min.js"></script>
```

3. Add using/imports if needed in `_Imports.razor`:

```razor
@using BlazorBlueprintUI
@using BlazorBlueprintUI.Components
```

4. Replace placeholder components with real Blueprint components (see the MCP server or package docs for component API and examples).

MCP server (optional)
- We added a project MCP config `.mcp.json` and a BMAD doc for the Blazor Blueprint MCP server. Start the server via:

```bash
npx @blazorblueprint/mcp@latest
```

Then use the MCP tools to browse components and blueprints.
