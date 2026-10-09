# Local Setup - Database Connection String

## 1. The setting name

```
ConnectionStrings:TalebElm
```

## 2. The default for local development

`src/TalebElm.Api/appsettings.Development.json` already contains:

```json
"ConnectionStrings": {
  "TalebElm": "Data Source=talebelm.db"
}
```

This creates a file called `talebelm.db` next to the running app. You do not
need to change anything to get started.

`appsettings.json` does **not** contain this setting on purpose. Outside of
`Development`, the connection string must be given by the environment.

## 3. Overriding it locally

If you want to use a different database file, set an environment variable.
It wins over the value in `appsettings.Development.json`.

**macOS / Linux:**

```
export ConnectionStrings__TalebElm="Data Source=my-other.db"
```

**Windows (PowerShell):**

```
$env:ConnectionStrings__TalebElm="Data Source=my-other.db"
```

These enviornment variables only last until you end the terminal session.

## 4. "Connection string 'TalebElm' does not exist or is empty"

You will see this error if the setting is missing or blank. To fix it, either:

- run the app in the `Development` environment (the default when you run it
  locally), so `appsettings.Development.json` is used, or
- set the `ConnectionStrings__TalebElm` environment variable as shown above.

## 5. Safety

- Database files (`*.db`, `*.db-shm`, `*.db-wal`) are ignored by git. Never
  commit them, they can contain real user data.
- Never put passwords or production connection strings in any
  `appsettings*.json` file, these files *are* committed to git.
- Before every commit, run `git diff` and check your changes.
