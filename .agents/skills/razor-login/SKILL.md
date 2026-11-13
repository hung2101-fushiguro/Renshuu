---
name: razor-login
description: Build or improve login and account flows in TracNghiemWeb using ASP.NET Core 8 Identity, Razor Pages and SQL Server, including accessible login UI, role authorization and per-user quiz history. Use for account/authentication work, not unrelated CRUD or import tasks.
---

# Razor Pages login

Read the repository AGENTS.md and inspect Program.cs, AppDbContext, the layout and site.css first. Apply this skill to the user's requested account features; registration, password reset, email delivery and external providers are optional scope, not prerequisites for every login task.

## Project fit

- Keep ASP.NET Core 8 Razor Pages, EF Core 8 and SQL Server. Prefer ASP.NET Core Identity with cookie authentication for this server-rendered application. Match added package major versions to .NET 8. Do not introduce a SPA, JWT storage in the browser or an SDK upgrade just for login.
- Use UserManager and SignInManager for password and sign-in behavior. Do not store plaintext passwords, hand-roll hashing or treat session values as identity.
- Preserve the existing quiz schema and data when extending the DbContext. Inspect the migration before applying it and identify the target database under the repository's database rules.

## Access and ownership

- Protect Admin pages and their handlers with server-side role/policy authorization. Hiding navigation alone is insufficient. Public registration must never accept an Admin role from the form.
- If the requested feature includes individual quiz data, link attempts to the authenticated user and scope all reads and writes, including Take, Submit, Result, History, deletion and wrong-answer queries. Derive user ID from the authenticated principal, not form data.
- LatestAnswers, StartWrongAsync and subject wrong counts must use the same ownership filter. A guessed attempt ID must not expose or change another user's work.
- Existing attempts have no owner. Define an explicit migration strategy, such as preserving them as inaccessible legacy records; never silently assign shared history to a new user. Explain the chosen behavior before a database migration.
- Seed administrator access only through an explicit development setup using user secrets or environment variables. Do not commit passwords or promote the first registered account automatically.

## Sign-in behavior

- Keep Razor Pages antiforgery protection on forms; logout must be a POST. Use local URL validation or LocalRedirect for returnUrl.
- Configure cookie lifetime, HttpOnly, appropriate SameSite behavior and HTTPS protection for production. Register authentication before authorization and map endpoints afterward.
- Use generic credential errors and Identity lockout/rate limiting appropriate to the app. Do not disclose account existence or log submitted passwords/tokens.
- Enable remember-me only through a deliberate user choice. Use Identity's reset/confirmation tokens when those features are requested, and show usable flows only when email delivery is configured.

## Login interface

Use the sibling frontend-design skill for visual work. Fit the existing education/quiz design in design-system/tracnghiemweb/MASTER.md, Pages/Shared/_Layout.cshtml and wwwroot/css/site.css; user design choices take priority. Keep the form focused and Vietnamese text concise.

- Provide persistent labels, autocomplete hints (username/current-password), keyboard navigation, visible focus and readable contrast.
- Include password visibility with an accessible toggle, associated field errors and a summary for server errors. Preserve entered username after failure; never repopulate the password.
- Handle submitting and failure states, prevent accidental repeated submission and allow retry. Test narrow mobile widths and long validation messages.
- Do not add decorative statistics, fake testimonials, third-party sign-in buttons or password-reset links without working behavior.

## Evidence before completion

Build the project using AGENTS.md. For implemented authentication, verify successful and failed login, logout, protected-page redirects, non-Admin rejection, invalid return URLs and antiforgery handling. When ownership is added, verify two separate users cannot read, submit or delete each other's attempts. Use a disposable identified database for such checks. If prerequisites are missing, report which checks remain unrun.

Installing this skill alone requires no application startup, new packages or database changes.

## Primary references

Consult these .NET 8 references for the feature being implemented:
- [Identity and Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0)
- [Authorization and ownership](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-8.0)
- [Prevent open redirects](https://learn.microsoft.com/en-us/aspnet/core/security/preventing-open-redirects?view=aspnetcore-8.0)
