# Blind-Match PAS

Blind-Match PAS is a secure ASP.NET Core MVC Project Approval System for university modules. It implements a blind matching workflow where supervisors review anonymous student proposals first, then both parties are revealed only after a confirmed match.

## Features

- ASP.NET Core MVC with Razor Views
- Entity Framework Core with SQLite code-first setup for easy local running
- ASP.NET Core Identity authentication
- Role-based authorization for Student, Supervisor, and Admin
- Blind proposal review before confirmation
- Proposal lifecycle controls and state transition protection
- Admin oversight, manual reassignment, reports, audit log, and CSV export
- Responsive Bootstrap 5 UI with custom styling
- Unit and integration test projects

## Project Structure

```text
Blind_Match/
|-- BlindMatchPAS.sln
|-- README.md
|-- BlindMatchPAS/
|   |-- BlindMatchPAS.csproj
|   |-- Program.cs
|   |-- appsettings.json
|   |-- Controllers/
|   |-- Data/
|   |-- Extensions/
|   |-- Helpers/
|   |-- Interfaces/
|   |-- Models/
|   |-- Properties/
|   |-- Services/
|   |-- ViewModels/
|   |-- Views/
|   `-- wwwroot/
|-- BlindMatchPAS.Tests.Unit/
`-- BlindMatchPAS.Tests.Integration/
```

## Main Roles

- Student: register, submit proposals, update before allocation, track status, view supervisor after reveal
- Supervisor: manage expertise, browse anonymous proposals, express interest, confirm matches, view matched students
- Admin: manage users and research areas, review all proposals, manually assign or reassign supervisors, audit activity, export allocations

## Blind Matching Rules

- Before confirmation, supervisors can only see title, abstract, technical stack, and research area
- Student identity is hidden during the anonymous review stage
- When a supervisor confirms a match:
  - a `MatchRecord` is created
  - the proposal status changes to `Matched`
  - reveal timestamps are recorded
  - student and supervisor details become visible to each other
- Withdrawn proposals cannot be matched
- Matched proposals cannot be edited or withdrawn by students
- Admin can manually assign or reassign supervisors

## Default Seed Data

- Default admin account:
  - Email: `admin@blindmatchpas.local`
  - Password: `Admin@12345`
- Seeded research areas:
  - Artificial Intelligence
  - Cyber Security
  - Software Engineering
  - Data Science
  - Human Computer Interaction

## Technology Stack

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- Bootstrap 5
- xUnit, Moq, FluentAssertions

## Setup Instructions

1. Install the .NET 8 SDK.
2. Open the folder in Visual Studio Code.
4. Restore packages.
5. Create the first migration.
6. Update the database.
7. Run the app.

## Connection String

The default connection string is stored in [appsettings.json](/c:/Users/AAt/Documents/Blind_Match/BlindMatchPAS/appsettings.json).

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=blindmatchpas.db"
}
```

## EF Core Migrations

The project is prepared for migrations through [ApplicationDbContextFactory.cs](/c:/Users/AAt/Documents/Blind_Match/BlindMatchPAS/Data/ApplicationDbContextFactory.cs) and the commands below.

Create and apply migrations:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet ef database update --project .\BlindMatchPAS\BlindMatchPAS.csproj
```

## Testing

### Unit Tests

Implemented in [BlindMatchPAS.Tests.Unit](/c:/Users/AAt/Documents/Blind_Match/BlindMatchPAS.Tests.Unit):

- cannot reveal identity before confirmation
- confirm match updates status correctly
- cannot match withdrawn proposal
- cannot match already matched proposal
- Moq is used to verify audit logging interactions

### Integration Tests

Implemented in [BlindMatchPAS.Tests.Integration](/c:/Users/AAt/Documents/Blind_Match/BlindMatchPAS.Tests.Integration):

- proposal creation persists correctly
- research area linking works
- match creation updates database state

### Functional Testing Structure

Suggested functional testing checklist:

1. Register a student and submit a proposal.
2. Register or create a supervisor and assign expertise.
3. Confirm the supervisor can browse the proposal anonymously.
4. Confirm no student identity is visible before match confirmation.
5. Express interest and confirm the match.
6. Verify reveal of student data to supervisor and supervisor data to student.
7. Verify admin manual reassignment updates the active match.
8. Verify unauthorized users receive the access denied page.

## NuGet Packages

Main project:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.UI`

Test projects:

- `xunit`
- `Moq`
- `FluentAssertions`
- `Microsoft.EntityFrameworkCore.InMemory`
- `Microsoft.EntityFrameworkCore.Sqlite`

## Coursework Coverage

This project meets the coursework brief by providing:

- secure authentication and role-based access control
- blind matching workflow with identity reveal only after confirmation
- complete proposal and allocation lifecycle management
- dashboards for all required roles
- research area and user administration
- reporting, auditing, and export support
- unit and integration testing structure

## Suggested Git Commit Plan

1. `chore: scaffold solution and mvc structure`
2. `feat: add identity models and ef core data layer`
3. `feat: implement proposal and blind matching services`
4. `feat: add student supervisor and admin controllers`
5. `feat: build razor views and responsive ui`
6. `test: add unit and integration tests`
7. `docs: add readme and setup instructions`

## VS Code Terminal Commands

If you want to recreate this manually instead of using the provided files:

```powershell
dotnet new sln -n BlindMatchPAS
dotnet new mvc -n BlindMatchPAS
dotnet new xunit -n BlindMatchPAS.Tests.Unit
dotnet new xunit -n BlindMatchPAS.Tests.Integration
dotnet sln .\BlindMatchPAS.sln add .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet sln .\BlindMatchPAS.sln add .\BlindMatchPAS.Tests.Unit\BlindMatchPAS.Tests.Unit.csproj
dotnet sln .\BlindMatchPAS.sln add .\BlindMatchPAS.Tests.Integration\BlindMatchPAS.Tests.Integration.csproj
dotnet add .\BlindMatchPAS\BlindMatchPAS.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add .\BlindMatchPAS\BlindMatchPAS.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add .\BlindMatchPAS\BlindMatchPAS.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add .\BlindMatchPAS\BlindMatchPAS.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add .\BlindMatchPAS\BlindMatchPAS.csproj package Microsoft.AspNetCore.Identity.UI
dotnet add .\BlindMatchPAS.Tests.Unit\BlindMatchPAS.Tests.Unit.csproj package Moq
dotnet add .\BlindMatchPAS.Tests.Unit\BlindMatchPAS.Tests.Unit.csproj package FluentAssertions
dotnet add .\BlindMatchPAS.Tests.Unit\BlindMatchPAS.Tests.Unit.csproj package Microsoft.EntityFrameworkCore.InMemory
dotnet add .\BlindMatchPAS.Tests.Integration\BlindMatchPAS.Tests.Integration.csproj package Moq
dotnet add .\BlindMatchPAS.Tests.Integration\BlindMatchPAS.Tests.Integration.csproj package FluentAssertions
dotnet add .\BlindMatchPAS.Tests.Integration\BlindMatchPAS.Tests.Integration.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet restore
dotnet ef migrations add InitialCreate --project .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet ef database update --project .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet run --project .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet test .\BlindMatchPAS.Tests.Unit\BlindMatchPAS.Tests.Unit.csproj
dotnet test .\BlindMatchPAS.Tests.Integration\BlindMatchPAS.Tests.Integration.csproj
```

## Important Note

The project now uses SQLite locally, so it can run in VS Code without installing SQL Server LocalDB.
