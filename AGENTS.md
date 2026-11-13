# TracNghiemWeb project guidance

## Project
- ASP.NET Core 8 Razor Pages application using EF Core 8 and SQL Server.
- The main user flows are question administration/import, quiz taking/submission, and result/history/wrong-answer review.
- `QuizService` owns quiz and attempt behavior; `ImportService` owns parsing, preview support, duplicate detection, and import persistence.

## Commands
- Restore: `dotnet restore TracNghiemWeb.csproj`
- Build: `dotnet build TracNghiemWeb.csproj --configuration Release --no-restore`
- Run locally: `dotnet run --project TracNghiemWeb.csproj`
- There is currently no test project. Do not claim tests ran; build the web project and report that tests are not configured.

## Database safety
- SQL Server connection comes from `ConnectionStrings:DefaultConnection` in configuration or environment overrides.
- Development startup automatically applies EF Core migrations and may seed a fresh database. Do not start the app or run migrations unless the task requires database changes and the target database is clear.
- Never expose connection strings, credentials, or user secrets in logs or commits. Do not change machine-specific server names as part of routine work.
- Inspect migration `Up` and `Down` before changing schema; preserve existing data and relationships.

## Implementation conventions
- Keep Razor Page handlers thin and put shared quiz/import behavior in the existing services.
- Follow existing async EF Core patterns, data annotations, TempData feedback, and Vietnamese UI wording.
- Preserve passage/question grouping and the latest-submitted-answer rule used for wrong-question practice.
- Validate user input on the server. For HTML output, use Razor encoding or the existing `Display.RichText` helper; do not bypass encoding for arbitrary content.
- Keep changes scoped. Do not add packages or a test framework unless requested or necessary for the feature.

## Change workflow
For account/login work, load `.agents/skills/razor-login/SKILL.md`. For new or redesigned UI, also load `.agents/skills/frontend-design/SKILL.md`. Preserve the existing Razor Pages stack and consult .NET 8 documentation. Installing these skills does not add authentication to the application.

1. Inspect applicable instructions, Git status, and the relevant page, service, model, and callers.
2. State the intended behavior and inspect neighboring conventions before editing.
3. Implement the smallest complete change; include a regression test when a test project exists and the behavior warrants one.
4. Review the full diff for data loss, migration, validation, security, and UI-flow effects.
5. Run the project build and any applicable tests. Report exact checks and environment limitations; never imply that an unrun check passed.

## Database handoff after every task
- After every task, update `database/TASK-LOG.md`. If there is no database change, explicitly record "Không cần SQL/migration".
- For schema changes, include the EF migration, regenerate `database/update-database.sql` with `dotnet ef migrations script --idempotent --project TracNghiemWeb.csproj --output database/update-database.sql`, and update `database/verify-database.sql` and `database/README.md`.
- Handoff instructions must let a teammate's agent identify its own effective server/database, inspect existing migration history, back up existing data, execute SQL with error checking, and verify migrations/schema and row counts afterwards. Never assume a teammate uses the same SQL Server instance.
- Report which database was actually updated; distinguish generated SQL from executed SQL. Preserve credentials outside Git and do not export account data in schema scripts.
