# TalebElm — MVP REST API Endpoints

> **Read this first:** this page lists the HTTP endpoints (web addresses) that
> the TalebElm API will answer for its **Minimum Viable Product (MVP)**.
>
> Every endpoint has three parts:
> - **Method** — the HTTP verb (`GET` reads, `POST` creates, `PUT` updates, `DELETE` removes).
> - **Route** — the web address. `{id}` means "put a real id here", for
>   example `/api/tracks/3f2a...`.
> - **Purpose** — what it does in plain English.

---

## 1. Authentication & Users

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/auth/register` | Create a new user account (student or instructor). Send name + email + password. |
| `POST` | `/api/auth/login` | Log in and get a token. Send email + password; the API returns a token the app uses on later requests. |
| `GET` | `/api/users` | List all users. Useful for admins and instructors. |
| `GET` | `/api/users/{id}` | Get one user by its id. |
| `GET` | `/api/users/me` | Get the currently logged-in user's own profile. |

> The `AuthController` (Tasks 36–42) starts as empty actions; the login logic
> is added after the MVP structure is complete.

---

## 2. Tracks & Content Management

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/tracks` | List all published tracks (the home page). |
| `GET` | `/api/tracks/{id}` | Get one track by its id, including its modules in order. |
| `POST` | `/api/tracks` | Create a new track (instructors/admins). Send name, description, status. |
| `PUT` | `/api/tracks/{id}` | Update a track's name, description, or status. |
| `DELETE` | `/api/tracks/{id}` | Remove a track (admins only). |
| `GET` | `/api/tracks/{id}/modules` | List the modules of one track, ordered by their `Order` number. |
| `POST` | `/api/modules` | Add a module to a track. Send title, summary, order, track id. |
| `PUT` | `/api/modules/{id}` | Update a module's title, summary, or order. |
| `GET` | `/api/modules/{id}` | Get one module by its id. |
| `GET` | `/api/modules/{id}/lessons` | List the lessons of one module, ordered by their `Order` number. |
| `POST` | `/api/lessons` | Add a lesson to a module. Send title, content, order, module id. |
| `GET` | `/api/lessons/{id}` | Get one lesson by its id (its full content). |

> The `TracksController`, `ModulesController`, and `LessonsController` already
> exist as empty tasks (Tasks 36–39). The create/update/delete actions are
> added once the structure phase is merged.

---

## 3. Assessments & Exams

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/exams/{id}` | Get the questions of one module's exam. |
| `POST` | `/api/exams/{id}/submit` | Submit the user's answers. The API checks the score against the exam's pass threshold and returns the result (passed / failed + score). |
| `GET` | `/api/progress/me` | Get the logged-in user's progress: which modules are unlocked, which exams are passed. |
| `GET` | `/api/progress/me/tracks/{id}` | Get the user's progress inside one specific track. |

> **Progression lock reminder:** the `submit` endpoint is where the lock
> happens. When the user passes an exam, the API auto-unlocks the next module
> by updating the user's `UserProgress` (`IsUnlocked = true`). See
> `docs/DATABASE_ERD.md` for how the tables store this.

---

## Naming & style rules

- Routes use **kebab-case** (lowercase, dash-separated words).
- Ids are **GUIDs**, e.g. `3f2a5c8e-...`.
- Data moves in and out as **JSON**.
- A response usually includes an `id` so the client can call detail routes.

## Response status codes

| Code | Meaning |
|---|---|
| `200 OK` | The request worked and returned data. |
| `201 Created` | A new resource (track, module, lesson...) was created. |
| `400 Bad Request` | The input was missing or invalid. |
| `401 Unauthorized` | No token, or the token is wrong. |
| `403 Forbidden` | The user is logged in but not allowed to do this. |
| `404 Not Found` | The id does not match anything. |