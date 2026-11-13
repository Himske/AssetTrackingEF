# Asset Tracking Pro

A .NET 10 console application for managing computer and mobile assets across offices. It uses Entity Framework Core with SQL Server for persistence and Spectre.Console for its interactive menus, tables, and Figlet header.

## Features

- **Assets:** add, list, update, and delete assets. Supported asset types are Laptop, Desktop, Mobile, and Tablet.
- **Asset filters:** view expired assets, computers, mobile devices, or assets assigned to a selected office.
- **Asset search:** search by partial brand or model, select an office, or search by purchase year.
- **Offices:** add, list, rename, and delete offices. Each office has a name, country, and currency code.
- **Reports:**
  - list assets and total purchase value by office, displayed in that office's currency;
  - show asset counts by office, assets near expiration or expired, and assets ranked by purchase price.
- **Report export:** export either report as TXT, CSV, or JSON. Files are created in a `Reports` directory under the application's current working directory, with timestamped names.
- **Administration:** seed sample data, empty the database, and refresh currency rates.
- **Currency rates:** rates are downloaded from the [fawazahmed0 currency API](https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.json). If the request fails or returns invalid JSON, the app uses the bundled `Data/usd.json` file.

Asset purchase prices are stored in USD. Conversion and display in an office's local currency require currency rates in the database; use **Admin > Refresh Currency Rates** to load them. Rate refresh needs network access unless the bundled fallback is used.

Assets are considered expired after three years. The status display also marks assets approaching that age as `RED` or `YELLOW`.

## Requirements

- .NET 10 SDK
- SQL Server Express running locally as `SQLEXPRESS`, or a SQL Server connection configured for your environment
- Windows or another terminal that supports UTF-8 output for currency symbols and the Figlet header

The current connection string is defined in `Data/AppDbContext.cs` and uses Windows integrated authentication for `localhost\\SQLEXPRESS`, with the database named `asset_tracking_ef`. Change it to suit your SQL Server setup before running if needed. Do not commit credentials or environment-specific secrets.

## Build and run

From the repository root:

```powershell
dotnet build AssetTrackingEF.slnx
dotnet run --project AssetTrackingEF.csproj
```

The application creates the database and schema if they do not already exist by calling EF Core `EnsureCreated`. This does **not** apply the migrations in the `Migrations` directory. The migrations are retained in the repository, but the current startup schema-management path is `EnsureCreated`.

## Getting started

1. Ensure SQL Server is available and the connection string in `Data/AppDbContext.cs` is correct.
2. Build and run the project.
3. Choose **Admin > Seed Database** to create the sample offices and assets. Seeding is intended for an empty database; it skips seeding when assets already exist.
4. Choose **Admin > Refresh Currency Rates** to load rates for local-currency display and conversions.
5. Use **Asset**, **Office**, or **Reporting** from the main menu to manage or review data.

The bundled `Data/usd.json` is copied to the build output and is used as the offline fallback for currency-rate refreshes.

## Menu guide

### Asset

- **Add Asset** collects type, brand, model, purchase date, USD purchase price, and serial number, then prompts for an office by name.
- **Show All Assets** displays the list and provides filters for expired assets, computers, mobile devices, a selected office, or all assets.
- **Search** supports brand, model, office, and purchase year.
- **Update Asset** and **Delete Asset** operate on an asset ID shown in the list.

### Office

Add an office with its name, country, and ISO currency code; list offices; update an office name; or delete an office.

### Reporting

The first report groups assets by office and displays each asset's status and local-currency purchase price, followed by the office total. The second report contains office asset counts, assets near expiration or expired, and assets ordered by purchase price (highest first). After either report, choose whether to export it and select TXT, CSV, or JSON.

### Admin

- **Seed Database:** add the bundled sample offices and assets when the database has no assets.
- **Empty Database:** delete all assets, offices, and currency rates.
- **Refresh Currency Rates:** replace the stored rates with API data or the bundled JSON fallback.

## Project structure

```text
Controllers/   Asset, office, currency, administration, and reporting operations
Data/          EF Core DbContext and bundled USD currency-rate JSON
Helper/        Sample-data seeding
Migrations/    EF Core migration history and model snapshot
Models/        Asset, asset subtypes, office, and currency-rate entities
Views/         Spectre.Console menus, reports, and shared Figlet header
Program.cs     Application entry point and main-menu routing
```

The project currently has no test project. Build verification is available with the command above.