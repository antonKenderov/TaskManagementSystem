# Task Management System

A desktop task manager built with C# and WPF on .NET 10, backed by PostgreSQL.

Tasks carry a description, dates, a status, a type and an assignee. Each task holds
comments, and a comment can set a reminder date — the earliest reminder across a
task's comments becomes that task's next action date. Comments can be searched
across every task, and a dashboard summarises the current state of the work.

---

## Requirements

- Windows (WPF)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 17 — either through Docker Desktop, or an existing server

---Move the form state into NewTaskViewModel and CommentComposerViewModel. Each owns its fields, its validation and its own saving flag, and reports back through a callback the parent fills in - the same shape already used for opening a task from search, and for the same reason: a reference the other way would make the constructors circular.

## Getting started

### 1. Start the database

Copy the example environment file and set a password:

```bash
cp .env.example .env
```

Then:

```bash
docker compose up -d
```

This starts PostgreSQL 17 on **port 5433** (5432 inside the container) with a
database named `taskmanagement`. The port is deliberately not the default, so the
container does not clash with a PostgreSQL installed on the host.

### 2. Create the schema

Either apply the EF Core migrations:

```bash
dotnet ef database update --project TaskManagementSystem.Data --startup-project TaskManagementSystem.Data
```

In Visual Studio, the same thing from the Package Manager Console:

```powershell
Update-Database -Project TaskManagementSystem.Data -StartupProject TaskManagementSystem.Data
```

or run the generated script against the database:

```bash
docker compose exec -T postgres psql -U postgres -d taskmanagement < db/schema.sql
```

`db/schema.sql` is produced from the migrations with `--idempotent`, so it is safe
to run more than once. Both routes seed four users, who are the people a task can
be assigned to.

To regenerate the script after a schema change:

```bash
dotnet ef migrations script --idempotent --output db/schema.sql --project TaskManagementSystem.Data --startup-project TaskManagementSystem.Data
```

```powershell
Script-Migration -Idempotent -Output db\schema.sql -Project TaskManagementSystem.Data -StartupProject TaskManagementSystem.Data
```

The migration tooling reads its connection string from the `TASKMANAGER_CONNECTION`
environment variable:

```bash
setx TASKMANAGER_CONNECTION "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=<your password>"
```

Open a new terminal afterwards — `setx` only affects processes started later.

### 3. Configure the application

`TaskManagementSystem/appsettings.json` holds the connection string without a
password. Put the password in a local override that is not committed:

```
TaskManagementSystem/appsettings.Development.json
```

```json
{
  "ConnectionStrings": {
    "DatabaseConnection": "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=<your password>"
  }
}
```

### 4. Run

```bash
dotnet run --project TaskManagementSystem
```

In Visual Studio, set `TaskManagementSystem` as the startup project and press F5.

### Running against an existing PostgreSQL

Skip `docker compose`, create an empty database, run `db/schema.sql` against it,
and point `appsettings.Development.json` at your own host, port and credentials.

---

## Tests

```bash
dotnet test
```

In Visual Studio, run them from **Test Explorer** (Test → Test Explorer, then Run All).

The tests are integration tests: they start a throwaway PostgreSQL container with
[Testcontainers](https://dotnet.testcontainers.org/), apply the migrations to it
and run against real SQL. **Docker must be running.** They do not touch the
database used by the application.

An in-memory provider was deliberately not used — it would not catch the things
these tests exist for, such as the `DateTime.Kind` requirement of Npgsql, the
enum-to-text conversions or the foreign key behaviour.

---

## Project structure

```
TaskManagementSystem.Domain        entities and enums, no dependencies
TaskManagementSystem.Data          DbContext, entity configurations, migrations
TaskManagementSystem.Application   services and DTOs
TaskManagementSystem               WPF application: views and view models
TaskManagement.Tests               integration tests
```

Dependencies run in one direction: UI → Application → Data → Domain. The domain
project references nothing, and holds no persistence concerns — the mapping rules
live in `IEntityTypeConfiguration` classes in the data project.

The UI follows MVVM with `CommunityToolkit.Mvvm`. `App.xaml.cs` is the composition
root: it is the only place that resolves services from the container, and the only
place that names concrete implementations. Every view model receives what it needs
through its constructor.

Read paths project straight into DTOs inside the query, so the database returns
exactly the columns a screen shows. Writes go through `SaveChanges`, where an
override stamps `CreatedAt`, `ModifiedAt` and `ModifiedBy` on any entity that
implements `IAuditable`.

---

## Screens

**Dashboard** — totals by status, tasks due within 7 days, and deadlines falling in
the next 14 days.

**All Tasks** — the task list with filters for status, type and assignee, and a
form for creating a task. Clicking a row opens its detail.

**Task detail** — edit the task's fields, and add, edit or delete its comments. The
next action date recalculates as comments change.

**Search** — filter comments by keyword, comment type, when they were added and
when they remind, showing the task each comment belongs to. Double-clicking a
result opens that task. With no filters set, the four most recent comments are
listed.

---

## Assumptions

The task description left some things open. These are the readings I worked to:

- **"Keep track of changes (updates should be saved)"** was read as persisting
  edits, with `ModifiedAt` and `ModifiedBy` on tasks and comments, rather than a
  full version history.
- **There is no authentication**, so `ModifiedBy` records the Windows user name.
- **The user list is fixed.** Four users are seeded by the migration; the task
  allowed either a fixed or an editable list.
- **A required-by date in the past is rejected when creating a task but accepted
  when editing one.** An existing task may already be overdue, and refusing the
  save would block unrelated edits.
- **Comments have no author.** The model records what a comment says, its type and
  its reminder, but not who wrote it, so the search results identify a comment by
  its type instead.

---

## Known limitations

- **Search has no upper bound on results.** Fine at this scale; a real deployment
  would page.
- **Loading and row clicks are wired in code-behind.** Each screen triggers its
  initial load from a `Loaded` handler, and the task list opens a row from
  `PreviewMouseLeftButtonUp`. WPF has no built-in way to bind an event to a
  command; `Microsoft.Xaml.Behaviors.Wpf` would remove them.
