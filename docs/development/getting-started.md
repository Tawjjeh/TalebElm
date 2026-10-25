# TalebElm — Development Environment

> Status: current as of 2026-10-25.
> The supported development workflow is Linux, WSL2, or macOS (all .NET-capable environments work; there are no Docker or OS-specific runtime requirements).

---

## 1. Prerequisites

| Tool | Version | Why |
|---|---|---|
| .NET SDK | 10.x (check with `dotnet --version`) | Build, test, and run the API |
| Git | Current | Source control |
| IDE or editor | VS Code + C# Dev Kit, JetBrains Rider, or Visual Studio | Editing, debugging, navigation |
| SQLite viewer | Optional (e.g. DB Browser for SQLite) | Inspect the local `talebelm.db` |

There is no Docker, Caddy, or external database required for local development. TalebElm uses **SQLite** for development and integration tests.

---

## 2. First checkout

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

Verify the server is running:

```bash
curl http://localhost:5000/api/health
```

Expected response: `{"status":"healthy"}` (or similar).

---

## 3. Local configuration

Configuration uses `appsettings.Development.json`. Do not commit secrets or production values.

| Setting | Source | Default |
|---|---|---|
| `ConnectionStrings:TalebElm` | `appsettings.Development.json` | `Data Source=talebelm.db` |
| `ConnectionStrings__TalebElm` | Environment variable (overrides JSON) | — |

The database file (`talebelm.db`) is created automatically next to the running app when you first run the server or apply migrations. It is gitignored and must never be committed.

To use a different file:

```bash
# macOS / Linux
export ConnectionStrings__TalebElm="Data Source=my-other.db"

# Windows (PowerShell)
$env:ConnectionStrings__TalebElm="Data Source=my-other.db"
```

Rules:

- `appsettings.json` carries **no** connection string on purpose. Non-development environments must inject one via the environment.
- `*.db`, `*.db-shm`, `*.db-wal` files are gitignored. Never commit them.
- Never put real passwords, tokens, or production connection strings in `appsettings*.json`.

---

## 4. Database workflow

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/TalebElm.Infrastructure \
  --startup-project src/TalebElm.Infrastructure

# Apply pending migrations to the local database
dotnet ef database update \
  --project src/TalebElm.Infrastructure \
  --startup-project src/TalebElm.Infrastructure
```

Rules:

- Migrations are generated from `TalebElm.Infrastructure` only.
- Every migration is reviewed as code before merge.
- Do not use `EnsureCreated`.
- Do not edit a migration after it has been applied without reverting first.
- Integration tests create their own in-memory or SQLite database; they do not share the developer database.
- To reset local data, delete the `.db` file and re-run `dotnet ef database update`.

---

## 5. Running the tests

```bash
# All tests
dotnet test TalebElm.slnx

# Release mode (matches CI)
dotnet test TalebElm.slnx --configuration Release
```

There are no opt-in test categories or external service requirements. All tests use SQLite in-process and require no network access.

See [`testing.md`](testing.md) for coverage expectations, test layout, and the definition of done for tests.

---

## 6. Verify before opening a pull request

```bash
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
python3 scripts/check-docs.py
```

All four commands must pass with no errors. CI enforces the same gates. Do not open a PR with a failing build or test.

---

## 7. Troubleshooting

| Problem | Check |
|---|---|
| `Connection string 'TalebElm' does not exist` | Are you running in the `Development` environment? Is `appsettings.Development.json` present? |
| `No migrations to apply` | Run `dotnet ef database update` after a fresh clone |
| Build errors after a pull | Run `dotnet restore TalebElm.slnx` first; a new NuGet package may have been added |
| Tests fail with migration error | Delete `talebelm.db` and re-run the test; an old schema may conflict |
| Port 5000 already in use | Change the port in `Properties/launchSettings.json` or kill the process holding the port |
| `check-docs.py` reports a broken link | The file or section referenced was renamed; update the link in the same PR as the rename |

---

## 8. IDE and formatting

Recommended setup:

- JetBrains Rider or VS Code with C# Dev Kit.
- Enable analyzers and format on save.
- Use the `.editorconfig` in the repository root.
- Do not reformat unrelated files in a feature pull request.

The project targets **nullable reference types enabled**. New code must not introduce nullable warnings.

---

## 9. Onboarding checklist

- [ ] .NET 10 SDK installed (`dotnet --version` → `10.x`).
- [ ] `dotnet restore TalebElm.slnx` passes.
- [ ] `dotnet build TalebElm.slnx` passes with 0 warnings, 0 errors.
- [ ] `dotnet test TalebElm.slnx` passes.
- [ ] `dotnet run --project src/TalebElm.Api` starts and `GET /api/health` returns a healthy response.
- [ ] `python3 scripts/check-docs.py` passes.
- [ ] No `.db` file, secret, or connection string is staged for commit.

---

## 10. Development safety rules

- Use disposable local data only.
- Do not point your local development setup at a shared or production database.
- Do not disable authentication checks locally — use a test fixture or a development-mode stub instead.
- Do not commit generated keys, tokens, or database files.
- Report a suspected secret commit immediately and rotate the value.
