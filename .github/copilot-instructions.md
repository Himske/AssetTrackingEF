# Copilot instructions

## Project overview

- This is a .NET 10 C# console application for tracking computer and mobile assets, using Entity Framework Core with SQL Server and Spectre.Console.
- Keep responsibilities in the existing folders: `Models` for entities, `Data` for `AppDbContext`, `Controllers` for operations, and `Views` for console output.
- The application starts from top-level statements in `Program.cs` and routes to `AssetView`, `OfficeView`, and `AdminView` menus.

## Implementation conventions

- Preserve nullable reference types and implicit usings as configured in `AssetTrackingEF.csproj`.
- Follow the existing C# formatting and naming conventions; keep changes focused and consistent with the current controller/model/view separation.
- Keep asset subtype behavior consistent with `ComputerAsset` and `MobileAsset`, which both inherit from `Asset`.
- Keep office-related behavior consistent with the `Office` model and `OfficeController`/`OfficeView` flow.
- The context uses SQL Server and the project has EF Core migration files. When changing the EF model, keep the migration and model snapshot consistent; use EF tooling rather than hand-editing generated migration code. The current application calls `EnsureCreated`, which creates a database but does not apply migrations, so do not change the schema-management approach unless the task requires it.
- Do not add credentials or machine-specific connection details to source files.

## Build and run

- Build with `dotnet build AssetTrackingEF.slnx`.
- Run with `dotnet run --project AssetTrackingEF.csproj`.
- There is currently no test project in the solution; if tests are added, include focused coverage for the changed behavior.
