---
name: tracnghiemweb
description: Project-specific workflow for implementing, reviewing, and verifying changes in this ASP.NET Core Razor Pages quiz app.
---

# TracNghiemWeb development workflow

Use this skill when implementing or reviewing changes in this repository. First read the root `AGENTS.md` and follow its database-safety and project-specific rules.

## Understand the feature flow

- Admin subject/question management: `Pages/Admin` and `Models/Subject.cs`, `Models/Question.cs`.
- Bulk question and reading imports: `Pages/Admin/AddMany`, `Import`, `Paste`, `Readings`, plus `Services/ImportService.cs`.
- Taking and grading quizzes: `Pages/Index`, `Pages/Test/Take`, `Services/QuizService.cs`, and `Models/Attempt*.cs`.
- Results, history, and wrong-answer practice: `Pages/Test/Result`, `Pages/History`, `Pages/Wrong`, and the corresponding query methods in `QuizService`.
- Persistence and startup: `Data/AppDbContext.cs`, `Data/Migrations`, `Data/DbSeeder.cs`, and `Program.cs`.

Trace the Razor Page handler to its service and EF Core query before changing behavior. Check all callers of shared service methods.

## Implement safely

- For login, registration, account access, or authorization changes, use the sibling `razor-login` skill. For UI design changes, combine it with `frontend-design`. These are local skills in `.agents/skills/`; follow the project's .NET 8 and Razor Pages constraints over generic examples.

- Keep request handling in PageModels and reusable business rules in the existing services.
- Preserve validation and Vietnamese user-facing messages.
- Treat import preview data as untrusted input; keep validation and duplicate checks server-side.
- Preserve reading passage relationships when selecting, displaying, and grading quiz questions.
- Wrong-question practice is based on the most recent submitted answer per question; verify query semantics if changing this behavior.
- For schema changes, add an EF Core migration and inspect both directions. Do not run against a live or ambiguous database.

## Verify

1. Run `dotnet restore TracNghiemWeb.csproj` if dependencies or project files changed.
2. Run `dotnet build TracNghiemWeb.csproj --configuration Release` after implementation.
3. Run tests only when a test project exists; report that the repository currently has none otherwise.
4. Inspect `git diff` and `git status`; preserve unrelated user files and never push unless asked.

The GitHub Actions workflow at `.github/workflows/dotnet-build.yml` performs restore and Release build. It intentionally does not call `dotnet test` because no test project exists yet.
