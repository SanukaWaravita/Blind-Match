# Migrations

This folder is prepared for EF Core code-first migrations.

The project uses SQLite for local development.

To recreate migrations, run:

```powershell
dotnet restore
dotnet ef migrations add InitialCreate --project .\BlindMatchPAS\BlindMatchPAS.csproj
dotnet ef database update --project .\BlindMatchPAS\BlindMatchPAS.csproj
```
