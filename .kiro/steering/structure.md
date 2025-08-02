# Project Structure & Architecture

## Layered Architecture Pattern

The Matrix project follows a clean layered architecture with clear separation of concerns:

```
Controllers → Services → Data/Models → Database
     ↓           ↓          ↓
   DTOs    →  Business  →  Entity
(Input/Output) Logic    Configuration
```

## Folder Organization

### Core Application Layers
- **`Controllers/`** - HTTP request handlers, thin layer that coordinates between views and services
- **`Services/`** - Business logic layer, contains core application functionality
  - **`Services/Interfaces/`** - Service contracts and abstractions
- **`Models/`** - Entity models representing database tables
- **`DTOs/`** - Data Transfer Objects for API input/output and inter-layer communication
- **`Data/`** - Database context and configuration
  - **`Data/Configurations/`** - Entity Framework Fluent API configurations

### Presentation Layer
- **`Views/`** - Razor view templates organized by controller
  - **`Views/Shared/`** - Shared layouts, partials, and common views
- **`ViewModels/`** - View-specific data models
- **`wwwroot/`** - Static web assets (CSS, JS, images, fonts)
  - **`wwwroot/css/`** - Compiled CSS files
  - **`wwwroot/scss/`** - SCSS source files
  - **`wwwroot/js/`** - JavaScript files
  - **`wwwroot/lib/`** - Client-side libraries (managed by LibMan)

### Localization
- **`Resources/`** - Localization resource files (.resx)
  - **`Resources/Views/`** - View-specific translations organized by controller/action

### Database
- **`Migrations/`** - Entity Framework migration files

## Key Conventions

### Naming Patterns
- **Controllers**: `{Entity}Controller.cs` (e.g., `AuthController.cs`)
- **Services**: `{Entity}Service.cs` with corresponding `I{Entity}Service.cs` interface
- **Models**: Singular entity names (e.g., `User.cs`, `Article.cs`)
- **DTOs**: `{Action}{Entity}Dto.cs` (e.g., `CreateUserDto.cs`, `UpdateArticleDto.cs`)
- **Views**: Organized in folders matching controller names

### Entity Relationships
- **User ↔ Person**: One-to-one relationship (User for auth, Person for profile)
- **UUID Primary Keys**: Custom UUID implementation for ordered, unique identifiers
- **Lazy Loading**: Enabled via Entity Framework Proxies

### Service Layer Patterns
- Services implement interfaces for testability and dependency injection
- Services handle business logic, validation, and data transformation
- Controllers remain thin, delegating to services
- DTOs used for data transfer between layers

### Configuration Management
- Environment variables loaded via DotNetEnv (.env file)
- Database connection strings built dynamically from environment
- JWT configuration in appsettings.json
- Localization configured for en-US and zh-TW

### Frontend Architecture
- Server-side rendering with Razor views
- Tailwind CSS + DaisyUI for styling
- SCSS for custom component styles
- jQuery for basic interactivity
- Vue.js available for complex components