# Dealer Database – Starter Solution

This is my attempt at the Jigsaw Finance technical task - Chris Eden

## Prerequisites

- .NET 8 SDK

## Structure

```
DealerDatabase.sln
data/                          Source data files to import
src/
  DealerDatabase.Data/         EF Core DbContext, entities and migrations (SQLite)
  DealerDatabase.DataAccess/   Repository classes for data access
  DealerDatabase.Import/       Console application that runs the import
  DealerDatabase.Models/       Shared model classes and the Schemas for the files to be read in
  DealerDatabase.Services/     Services for data readers, factory, ingestion, reconciliation and matching
  DealerDatabase.Web/          ASP.NET Core MVC web interface - simple Grid to display the dealers and some of their data
```

Both applications use the same SQLite database file, `dealers.db`, which is created in the solution root. `SolutionPaths` in the Data project resolves this location along with the `data/` folder.

## Running

Run the import:

```
dotnet run --project src/DealerDatabase.Import
```

Run the web interface:

```
dotnet run --project src/DealerDatabase.Web
```

Both applications apply any outstanding migrations on startup.

## Migrations

The EF Core CLI is included as a local tool. To restore it:

```
dotnet tool restore
```

To add a migration after changing the model:

```
dotnet ef migrations add <MigrationName> --project src/DealerDatabase.Data
```

The `dealers.db` file should already exist and migrations have been ran.

To start again with a fresh database, delete `dealers.db` and run either application.

## Notes

- The `Dealer` entity is no longer minimal. It has been changed and extended as I saw fit. linked entities have been created to hold further information for the Dealer
- I may have gone over the top with the matching and reconciliation logic, however, I have thoroughly enjoyed the challenge of implementing it and I think it is a good solution to the problem.
- Coding should always be fun.
