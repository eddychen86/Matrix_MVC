# Technology Stack

## Framework & Runtime
- **ASP.NET Core 8.0** - Main web framework
- **.NET 8.0** - Target framework with nullable reference types enabled
- **Entity Framework Core 8.0** - ORM with SQL Server provider and lazy loading proxies

## Database
- **SQL Server** - Primary database
- **Entity Framework Migrations** - Database schema management

## Authentication & Security
- **JWT Bearer Authentication** - Token-based authentication
- **Custom User/Person model** - No ASP.NET Identity, custom implementation
- **Password hashing** - SHA256 with salt

## Frontend Technologies
- **Razor Views** - Server-side rendering
- **Tailwind CSS** - Utility-first CSS framework
- **DaisyUI** - Component library for Tailwind
- **SCSS** - CSS preprocessing
- **jQuery** - JavaScript library
- **Vue.js** - Frontend framework (available in lib)

## Localization
- **ASP.NET Core Localization** - Multi-language support (en-US, zh-TW)
- **Resource files (.resx)** - Translation management

## Development Tools
- **LibMan** - Client-side library management
- **Tailwind CLI** - CSS compilation
- **DotNetEnv** - Environment variable management

## Common Commands

### Setup
```bash
# Install LibMan (if not using Visual Studio)
dotnet tool install Microsoft.Web.LibraryManager.Cli

# Restore client-side libraries
dotnet tool run libman restore

# Install required packages
dotnet add package DotNetEnv
dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 8.0.11
```

### Database
```bash
# Add migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# List packages
dotnet list package
```

### Frontend Build
```bash
# macOS - Watch Tailwind CSS
./tw.sh

# Windows - Watch Tailwind CSS  
.\tw.bat
```

### Development
```bash
# Run application
dotnet run

# Build application
dotnet build

# Create new view
dotnet new view -n <ViewName> -o <TargetFolder>
```