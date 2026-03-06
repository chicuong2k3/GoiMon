<!-- BMAD:START -->

# BMAD Method — Project Instructions

## Project Configuration

- **Project**: GoiMon
- **User**: Chicuong
- **Communication Language**: English
- **Document Output Language**: English
- **User Skill Level**: intermediate
- **Output Folder**: {project-root}/\_bmad-output
- **Planning Artifacts**: {project-root}/\_bmad-output/planning-artifacts
- **Implementation Artifacts**: {project-root}/\_bmad-output/implementation-artifacts
- **Project Knowledge**: {project-root}/docs

## BMAD Runtime Structure

- **Agent definitions**: `_bmad/bmm/agents/` (BMM module) and `_bmad/core/agents/` (core)
- **Workflow definitions**: `_bmad/bmm/workflows/` (organized by phase)
- **Core tasks**: `_bmad/core/tasks/` (help, editorial review, indexing, sharding, adversarial review)
- **Core workflows**: `_bmad/core/workflows/` (brainstorming, party-mode, advanced-elicitation)
- **Workflow engine**: `_bmad/core/tasks/workflow.xml` (executes YAML-based workflows)
- **Module configuration**: `_bmad/bmm/config.yaml`
- **Core configuration**: `_bmad/core/config.yaml`
- **Agent manifest**: `_bmad/_config/agent-manifest.csv`
- **Workflow manifest**: `_bmad/_config/workflow-manifest.csv`
- **Help manifest**: `_bmad/_config/bmad-help.csv`
- **Agent memory**: `_bmad/_memory/`

## Key Conventions

- Always load `_bmad/bmm/config.yaml` before any agent activation or workflow execution
- Store all config fields as session variables: `{user_name}`, `{communication_language}`, `{output_folder}`, `{planning_artifacts}`, `{implementation_artifacts}`, `{project_knowledge}`
- MD-based workflows execute directly — load and follow the `.md` file
- YAML-based workflows require the workflow engine — load `workflow.xml` first, then pass the `.yaml` config
- Follow step-based workflow execution: load steps JIT, never multiple at once
- Save outputs after EACH step when using the workflow engine
- The `{project-root}` variable resolves to the workspace root at runtime
- Prefer project-level global using directives: create a `GlobalUsings.cs` in each project containing common `global using` statements. When generating or refactoring C# code, prefer adding shared namespaces to the project `GlobalUsings.cs` rather than repeating file-level `using` statements. Only add file-level `using` directives when a namespace is highly specific to a single file.
- **Mandatory NuGet Libraries** — The following packages are **required** across the entire GoiMon project. ALWAYS use them; NEVER replace them with alternatives or raw implementations:
    - `BlazorBlueprint.Components` — UI component library (see BlazorBlueprint rule below)
    - `BlazorBlueprint.Icons.Lucide` — Icon library (`<LucideIcon Name="..." />`)
    - `StrawberryShake.Blazor` (v15.\*) — **GraphQL client**. All API communication MUST go through the auto-generated `GoiMonClient`. NEVER use raw `HttpClient` for GraphQL calls. Define `.graphql` operation files and let StrawberryShake generate typed C# clients.
    - `StrawberryShake.Blazor` (v15.\*) — **GraphQL client**. All API communication MUST go through the auto-generated `GoiMonStaff`. NEVER use raw `HttpClient` for GraphQL calls. Define `.graphql` operation files and let StrawberryShake generate typed C# clients.
    - `EasyAppDev.Blazor.Store` (v2.0.11) — **State management**. Use `IStateReader<T>` / `IStateWriter<T>` for UI state. Register stores via `AddScopedStoreWithUtilities()` in `Program.cs`.
- **ALWAYS use BlazorBlueprint components** for ALL UI in GoiMon.Client: `BbButton`, `BbCard`, `BbCardHeader`, `BbCardTitle`, `BbCardDescription`, `BbCardContent`, `BbCardFooter`, `BbInput`, `BbInputOTP`, `BbLabel`, `BbAlert`, `BbAlertTitle`, `BbAlertDescription`, `BbSeparator`, `BbSpinner`, `BbRadioGroup`, `BbBadge`, `BbSwitch`, `BbCheckbox`, `BbTextarea`, `BbTypography*`, `LucideIcon`, etc. NEVER use raw Bootstrap classes (`btn`, `card`, `alert`, `form-control`) or raw HTML `<button>`, `<input>`, `<div class="card">` when a BlazorBlueprint component exists. The component library is imported globally via `_Imports.razor` (`@using BlazorBlueprint.Components` and `@using BlazorBlueprint.Icons.Lucide.Components`). Use Tailwind CSS utility classes (e.g., `flex`, `gap-2`, `p-6`) for layout — this is what BlazorBlueprint uses internally. Key enums: `ButtonVariant.{Default,Secondary,Destructive,Outline,Ghost,Link}`, `ButtonSize.{Default,Small,Large,Icon,IconSmall}`, `AlertVariant.{Default,Success,Info,Warning,Danger}`, `SeparatorOrientation.{Horizontal,Vertical}`, `InputType.{Text,Number,Password,Email,...}`.
- **ALWAYS use BlazorBlueprint components** for ALL UI in GoiMon.Staff: `BbButton`, `BbCard`, `BbCardHeader`, `BbCardTitle`, `BbCardDescription`, `BbCardContent`, `BbCardFooter`, `BbInput`, `BbInputOTP`, `BbLabel`, `BbAlert`, `BbAlertTitle`, `BbAlertDescription`, `BbSeparator`, `BbSpinner`, `BbRadioGroup`, `BbBadge`, `BbSwitch`, `BbCheckbox`, `BbTextarea`, `BbTypography*`, `LucideIcon`, etc. NEVER use raw Bootstrap classes (`btn`, `card`, `alert`, `form-control`) or raw HTML `<button>`, `<input>`, `<div class="card">` when a BlazorBlueprint component exists. The component library is imported globally via `_Imports.razor` (`@using BlazorBlueprint.Components` and `@using BlazorBlueprint.Icons.Lucide.Components`). Use Tailwind CSS utility classes (e.g., `flex`, `gap-2`, `p-6`) for layout — this is what BlazorBlueprint uses internally. Key enums: `ButtonVariant.{Default,Secondary,Destructive,Outline,Ghost,Link}`, `ButtonSize.{Default,Small,Large,Icon,IconSmall}`, `AlertVariant.{Default,Success,Info,Warning,Danger}`, `SeparatorOrientation.{Horizontal,Vertical}`, `InputType.{Text,Number,Password,Email,...}`.
- **UI consistency rules (MUST follow)** — Keep typography and spacing consistent across all new/updated UI:
    - **Typography:** Use theme font tokens from `src/GoiMon.Client/wwwroot/css/theme.css` (`--font-sans`, `--font-serif`, `--font-mono`). Do NOT hardcode custom `font-family` per component/page.
    - **Typography:** Use theme font tokens from `src/GoiMon.Staff/wwwroot/css/theme.css` (`--font-sans`, `--font-serif`, `--font-mono`). Do NOT hardcode custom `font-family` per component/page.
    - **Spacing scale:** Prefer consistent Tailwind spacing steps (`gap-2/3/4`, `p-3/4/5`, `px-4/5`, `py-2/3`) and avoid arbitrary one-off values unless there is a proven layout requirement.
    - **Component sizing:** Use BlazorBlueprint size enums (`ButtonSize`, input sizes when available) instead of custom height/width hacks. For POS primary actions, prefer `ButtonSize.Large`.
    - **Color and visual tokens:** Use semantic tokens/classes (`bg-background`, `text-foreground`, `text-muted-foreground`, `border`, `bg-card`, `text-primary`) and existing variants. Never hardcode new hex/oklch values in Razor files.
    - **State consistency:** Active/selected/disabled/loading states must use existing component variants/patterns already present in nearby screens. Do not invent a new visual pattern for the same interaction type.
    - **Inline styles:** Avoid inline `style="..."` for spacing/typography/colors. Prefer tokens + utility classes; inline style is allowed only for unavoidable dynamic layout constraints.
- **Dialog componentization rules (MUST follow)** — Reduce duplicated dialog markup and keep dialog behavior consistent:
    - Reusable dialogs shared by 2+ screens MUST be extracted into shared components under `src/GoiMon.Client/Shared/Components/`.
    - Reusable dialogs shared by 2+ screens MUST be extracted into shared components under `src/GoiMon.Staff/Shared/Components/`.
    - Use a shared confirm dialog component for destructive confirmations (delete, bulk delete, irreversible actions) instead of duplicating `BbDialog` blocks in pages.
    - Use a shared form-dialog shell component for standard create/edit flows when header/footer/actions are similar and only body fields differ.
    - Keep feature-specific form dialogs as feature components in `Features/{FeatureName}/Components/` when the dialog has non-trivial business logic.
    - Pages should orchestrate open/close state and submit handlers; componentized dialogs should own presentation structure only.
    - For all new dialog components, follow spacing tokens `px-4 md:px-5`, `py-3/4`, and existing BlazorBlueprint variants for action hierarchy.
- **Feature-based folder structure** — Each feature MUST be self-contained in its own folder to avoid conflicts when multiple features are developed in parallel. Follow this convention:
    - **API** (`src/GoiMon.Api/`): `Features/{FeatureName}/` containing its own `Models/`, `Services/`, `Mutations/`, `Queries/`, `Types/`, `Validators/` sub-folders as needed. Domain entities go in `Domain/{AggregateName}/`.
    - **Client** (`src/GoiMon.Client/`): `Features/{FeatureName}/` containing `Components/`, `Models/`, `Services/`, `Helpers/`, `State/` sub-folders as needed. Pages go in `Pages/{FeatureName}/`. GraphQL operation files go in `GraphQL/{FeatureName}/`.
    - **Client** (`src/GoiMon.Staff/`): `Features/{FeatureName}/` containing `Components/`, `Models/`, `Services/`, `Helpers/`, `State/` sub-folders as needed. Pages go in `Pages/{FeatureName}/`. GraphQL operation files go in `GraphQL/{FeatureName}/`.
    - **Tests**: Mirror the feature structure — `tests/GoiMon.Api.Tests/Features/{FeatureName}/`, `tests/GoiMon.Client.Tests/Features/{FeatureName}/`.
    - **Tests**: Mirror the feature structure — `tests/GoiMon.Api.Tests/Features/{FeatureName}/`, `tests/GoiMon.Staff.Tests/Features/{FeatureName}/`.
    - **NEVER** place feature-specific code in shared/root folders (e.g., don't put authentication models in a generic `Models/` folder). Each feature owns its files.
    - **Shared code** (used by 2+ features) goes in `Shared/` or `Infrastructure/` folders — but only after confirming it is truly cross-cutting.
- **State management performance rules (MUST follow)** — Optimize `EasyAppDev.Blazor.Store` usage for render performance and maintainability:
    - **Prefer multiple focused stores** over one monolithic app/UI store. In GoiMon.Client, use domain stores (`CategoriesUiState`, `ProductsUiState`, `CombosUiState`, `OrdersUiState`, `CheckoutUiState`) and register each with `AddScopedStoreWithUtilities()`.
    - **Prefer multiple focused stores** over one monolithic app/UI store. In GoiMon.Staff, use domain stores (`CategoriesUiState`, `ProductsUiState`, `CombosUiState`, `OrdersUiState`, `CheckoutUiState`) and register each with `AddScopedStoreWithUtilities()`.
    - **Choose component base by interaction type:**
        - Use `StoreComponentWithUtilities<TState>` for pages/screens with substantial local UI state (dialogs, popovers, selection panels, form draft state, stepper/tab state) to avoid selector render-gating issues.
        - Use `SelectorStoreComponent<TState>` only for read-heavy components where selected store slices are the primary render trigger and local UI state is minimal.
    - **Selector safety rule:** If using `SelectorStoreComponent<TState>`, ensure local UI interactions are not blocked by selector gating. Do not use selector-based pages for dialog-heavy CRUD screens unless all required UI-render triggers are explicitly handled.
    - **Keep state minimal and derived**: avoid duplicating computable values in state records; expose computed/derived values as properties or dedicated derived records.
    - **Batch and debounce updates** for high-frequency events (search, scroll, resize). Prefer debounced update helpers or a single combined update over sequential updates, and ALWAYS apply debouncing wherever feasible.
    - **Debounced updates:** Prefer `UpdateDebounced` on `StoreComponentWithUtilities<TState>`. Use `IDebounceManager` for explicit key-based debounce flows where needed.
    - **Throttled updates:** Prefer `UpdateThrottled` on `StoreComponentWithUtilities<TState>`. Use `IThrottleManager` for explicit key-based throttle flows where needed.
    - **Always throttle continuous high-frequency events** (scroll, resize, pointer/mouse move, realtime stream bursts) wherever feasible to avoid UI/update storms.
    - **LazyLoad / cache dedupe for repeated reads:** Always apply lazy-load caching wherever feasible on repeated data-read paths (lookup lists, menu/reference data, tab/count queries) using `ILazyCache.GetOrLoadAsync(...)` with stable keys and appropriate TTL.
    - **ExecuteCachedAsync for concurrent load + state writes:** When repeated reads also trigger store state callbacks and may be called concurrently (same cache key), prefer `IAsyncActionExecutor<TState>.ExecuteCachedAsync(...)` over `LazyLoad + manual UpdateAsync/PersistStateAsync` so both fetch and state-callback updates are deduplicated.
    - **ExecuteCachedAsync invalidation:** After create/update/delete affecting cached keys, invalidate with `InvalidateCacheAsync(cacheKey)` or grouped invalidation via `InvalidateCacheByPrefixAsync(prefix)`; use `ClearCacheAsync()` on global reset/logout flows.
    - **Store boundaries by domain**: do not write unrelated page cache into another domain store. Keep cache payload and update action names scoped (`cache.categories`, `cache.products`, etc.).
    - **Current GoiMon.Client baseline (required)**: `Categories.razor`, `Products.razor`, `Combos.razor`, `Checkout.razor`, `Orders.razor` use `StoreComponentWithUtilities<TState>` with domain stores.
    - **Current GoiMon.Staff baseline (required)**: `Categories.razor`, `Products.razor`, `Combos.razor`, `Checkout.razor`, `Orders.razor` use `StoreComponentWithUtilities<TState>` with domain stores.
    - **Async/UI patterns**: optimistic updates are allowed only for reversible operations with clear rollback; for critical operations prefer server-confirmed updates.
    - **Performance validation**: before/after optimization, record render/update hotspots (component or event level) and verify no behavior regression.

## Available Agents

| Agent               | Persona     | Title                                                                | Capabilities                                                                             |
| ------------------- | ----------- | -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| bmad-master         | BMad Master | BMad Master Executor, Knowledge Custodian, and Workflow Orchestrator | runtime resource management, workflow orchestration, task execution, knowledge custodian |
| analyst             | Mary        | Business Analyst                                                     | market research, competitive analysis, requirements elicitation, domain expertise        |
| architect           | Winston     | Architect                                                            | distributed systems, cloud infrastructure, API design, scalable patterns                 |
| dev                 | Amelia      | Developer Agent                                                      | story execution, test-driven development, code implementation                            |
| pm                  | John        | Product Manager                                                      | PRD creation, requirements discovery, stakeholder alignment, user interviews             |
| qa                  | Quinn       | QA Engineer                                                          | test automation, API testing, E2E testing, coverage analysis                             |
| quick-flow-solo-dev | Barry       | Quick Flow Solo Dev                                                  | rapid spec creation, lean implementation, minimum ceremony                               |
| sm                  | Bob         | Scrum Master                                                         | sprint planning, story preparation, agile ceremonies, backlog management                 |
| tech-writer         | Paige       | Technical Writer                                                     | documentation, Mermaid diagrams, standards compliance, concept explanation               |
| ux-designer         | Sally       | UX Designer                                                          | user research, interaction design, UI patterns, experience strategy                      |

## Slash Commands

Type `/bmad-` in Copilot Chat to see all available BMAD workflows and agent activators. Agents are also available in the agents dropdown.

<!-- BMAD:END -->
