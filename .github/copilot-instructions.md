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
    - `EasyAppDev.Blazor.Store` (v2.0.11) — **State management**. Use `IStateReader<T>` / `IStateWriter<T>` for UI state. Register stores via `AddScopedStoreWithUtilities()` in `Program.cs`.
- **ALWAYS use BlazorBlueprint components** for ALL UI in GoiMon.Client: `BbButton`, `BbCard`, `BbCardHeader`, `BbCardTitle`, `BbCardDescription`, `BbCardContent`, `BbCardFooter`, `BbInput`, `BbInputOTP`, `BbLabel`, `BbAlert`, `BbAlertTitle`, `BbAlertDescription`, `BbSeparator`, `BbSpinner`, `BbRadioGroup`, `BbBadge`, `BbSwitch`, `BbCheckbox`, `BbTextarea`, `BbTypography*`, `LucideIcon`, etc. NEVER use raw Bootstrap classes (`btn`, `card`, `alert`, `form-control`) or raw HTML `<button>`, `<input>`, `<div class="card">` when a BlazorBlueprint component exists. The component library is imported globally via `_Imports.razor` (`@using BlazorBlueprint.Components` and `@using BlazorBlueprint.Icons.Lucide.Components`). Use Tailwind CSS utility classes (e.g., `flex`, `gap-2`, `p-6`) for layout — this is what BlazorBlueprint uses internally. Key enums: `ButtonVariant.{Default,Secondary,Destructive,Outline,Ghost,Link}`, `ButtonSize.{Default,Small,Large,Icon,IconSmall}`, `AlertVariant.{Default,Success,Info,Warning,Danger}`, `SeparatorOrientation.{Horizontal,Vertical}`, `InputType.{Text,Number,Password,Email,...}`.

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
