# CLAUDE.md - Matrix Project Master Prompt

This file provides comprehensive guidance to Claude Code when working with the Matrix repository. It covers not only the technical architecture but also the core brand identity, design principles, and user experience philosophy.

## 1. Brand & Vision (The "Why")

### 1.1 Project Name
Matrix

### 1.2 Brand Manifesto
This is the soul of the project. All design and communication should reflect this vision:

> The world is a fog, filled with out-of-focus noise.  
> We choose to become an eternal lighthouse.  
> Waiting for the echoes that can pierce the night.

### 1.3 Brand Introduction
Matrix is a sanctuary for Web3 pioneers and deep-tech enthusiasts, designed to filter out the noise of mainstream social media. We provide a pure, focused environment for high-quality discourse. Here, an on-chain credential is the only pass, ensuring a community built on expertise and shared consensus. While the platform may start with fragmented sparks of insight, we believe that as true peers connect, these unordered glimmers will form a grand and orderly matrix of thought.

Unlike platforms driven by entertainment, Matrix deliberately avoids gamification and unnecessary features. The focus is purely on fostering meaningful human-to-human interaction.

### 1.4 Core Values
- **Purity**: The platform is exclusively for discussions related to Web3 and deep-tech. All entertainment-focused content is actively discouraged.
- **Credibility**: User identity can be enhanced by linking a wallet address, granting a verification badge that builds trust within the community.
- **Order**: The community maintains high-quality discourse through a reporting system. Administrators have the authority to manage users and content to uphold platform standards.

### 1.5 Brand Tone of Voice
The brand's personality should be consistently applied across all user-facing text.

**Core Tone**: Mysterious, Poetic, Confident, Precise.

**Do's**: 
- Use brand-related metaphors (lighthouse, fog, echo, signal, key, journey)
- Maintain a calm and assertive tone
- Examples: "Welcome to the lighthouse.", "Verification complete."

**Don'ts**: 
- Avoid overly casual or slang terms
- Avoid aggressive marketing language
- Examples to avoid: "Hey! Welcome aboard!", "Sign up now for a premium experience!"

## 2. Visual Identity (The "Look and Feel")

### 2.1 Final Logo
The logo is a negative-space lighthouse within a circle, formed by organic, wave-like lines. It symbolizes guidance in the sea of digital fog.
(Reference the final logo file when designing UI components)

### 2.2 Color Palette
The color system is designed to be calm, professional, and tech-focused, with a single, vibrant accent for key actions.

| Use Case | Color Name | HEX Code |
|----------|------------|----------|
| Dark Base | Black | #082032 |
| Secondary Accent | Accent Navy | #2C394B |
| Borders/Text | Space Gray | #334756 |
| Primary Action | Orange | #FF4C29 |
| Light Base | White | #FFFFFF |

### 2.3 Typography
- **Primary Typeface**: Inter (English/Numeric), Noto Sans TC (Traditional Chinese)
- **Hierarchy**: A clear hierarchy from H1 (30px, Bold) to Body (15px, Regular) must be maintained

### 2.4 Iconography
- **Library**: Lucide (https://lucide.dev/) is the sole icon library
- **Style**: Icons must be line-style, with rounded corners to match the organic feel of the logo

### 2.5 Component Styling
- **Rounded Corners**: All components (cards, inputs, modals) must have soft, rounded corners (8px-12px)
- **Buttons**: All buttons must be pill-shaped (border-radius: 1000px)

## 3. Technical Architecture & Development (The "How")

### 3.0 Technology Stack

#### Backend Framework
- **ASP.NET Core 8.0** - Main web framework
- **Entity Framework Core 8.0.11** - ORM with SQL Server provider
- **DotNetEnv** - Environment variable management

#### Frontend Technologies
- **Tailwind CSS** - Utility-first CSS framework
- **DaisyUI** - Component library for Tailwind CSS
- **SCSS** - CSS preprocessor for component-based styling
- **PrimeVue** - Vue.js UI component library
- **Vue.js** - Progressive JavaScript framework for interactivity
- **Lucide Icons** - Icon library for consistent iconography

#### Development Tools
- **LibMan** - Client-side library management
- **Entity Framework Core Proxies** - Lazy loading support

### 3.1 Development Commands

#### Initial Setup
**Prerequisites**: Ensure you have .NET 8.0 SDK installed.

```bash
# Install LibMan CLI (if not using Visual Studio)
dotnet tool install Microsoft.Web.LibraryManager.Cli

# Restore client-side packages to wwwroot/lib folder
dotnet tool run libman restore

# Install required NuGet packages
# DotNetEnv: For automatic database connection using .env file
dotnet add package DotNetEnv

# Entity Framework Proxies: Enables lazy loading for automatic relationship loading
dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 8.0.11
```

**Note**: If using Visual Studio, DotNetEnv can be installed via NuGet Package Manager.

#### Running the Application
```bash
# 使用 launch profiles（推薦）
dotnet run --launch-profile http    # HTTP only (port 5000)
dotnet run --launch-profile https   # HTTPS + HTTP (ports 5001/5000)

# 標準方式
dotnet run

# 熱重載模式
dotnet watch run
```

#### Launch Profiles 配置
專案使用 `Properties/launchSettings.json` 定義啟動配置：
- **http**: `http://localhost:5002` (預設，避免與 AirTunes 衝突)
- **https**: `https://localhost:5001` + `http://localhost:5000`
- **IIS Express**: 使用 IIS Express (port 6447)

#### 內建診斷功能
**開發環境自動診斷：**
- 應用程式啟動時會自動檢查 port 衝突
- 顯示環境配置和資料庫連線狀態
- 記錄詳細的啟動資訊和訪問 URL
- 提供 403 問題的診斷提示

#### 常見問題排除
**403 錯誤或 port 衝突問題：**
- 通常是多個 dotnet 進程同時運行造成
- 開發環境下會自動顯示 port 衝突警告
- 手動清理：`pkill -f dotnet` 或使用工作管理員  
- 應用程式預設運行在 `http://localhost:5002`
- BE 管理後台：`http://localhost:5002/BE`
- 檢查 `Properties/launchSettings.json` 中的 port 配置
- **注意**: Port 5000 通常被 Apple AirTunes 佔用，建議使用其他 port

#### Database Operations
```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Check package information
dotnet list package
```

#### Frontend Asset Management
**Important**: This project uses DaisyUI UI Library, requiring Tailwind CSS CLI installation.

```bash
# Get Tailwind CSS executable for your OS

# Windows
curl -sLo tailwindcss.exe https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe

# macOS (ARM64)
curl -sLo tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-arm64

# Make executable (Linux/macOS)
chmod +x tailwindcss

# Watch CSS changes (runs Tailwind in background)
# macOS
./tw.sh

# Windows
.\tw.bat

# Create new view
dotnet new view -n <ViewName> -o <TargetFolder>

# Check EF tools and packages
dotnet list package
```

**Note**: DaisyUI bundled JS file is already included in the project.

### 3.2 Architecture Overview
The application follows a strict layered architecture:

```
Controllers → Services → Data/Repository → Database
     ↓          ↓           ↓
ViewModels ← DTOs ← Models/Entities
```

### 3.3 Key Architectural Components

This project follows a layered architecture pattern with clear separation of concerns:

#### Controllers (`/Controllers/`)
- **Role**: Entry points for incoming requests (web browser, API clients)
- **Function**: Handle user input and orchestrate responses
- **Key Principle**: Should be thin - no business logic or direct database interaction
- **Primary Role**: Coordinate between view/client and service layer
- **AuthController**: Handles authentication with custom routing (`[Route("/login")]`)

#### Services (`/Services/`)
- **Role**: The "brain" of the application containing core business logic
- **Function**: Process DTOs from Controllers and perform necessary actions:
  - Validate data based on business rules
  - Interact with database (via DbContext)
  - Call other services
  - Map data between DTOs and database models
- Uses interfaces for dependency injection
- **UserService**: Password hashing and user management
- **ArticleService**: Content management operations
- **NotificationService**: Notification handling

#### Models (`/Models/`)
- **Role**: "Blueprints" of data in the application
- **Function**: Define shape and relationships of data in C# world
- Entity definitions mapping to database tables (e.g., `Article.cs`)
- Uses UUID primary keys for enhanced security
- Complex relationships (User-Person 1:1, Article-Reply 1:many, etc.)
- Entities: User, Person, Article, Reply, Friendship, Follow, Notification, Report

#### Data Layer (`/Data/Configurations/`)
- **Role**: "Construction manual" for the database
- **Function**: Define how Models are mapped to actual database tables using Fluent API
- Provide detailed instructions to Entity Framework Core for schema building:
  - Primary keys setup
  - Column properties (max length, indexes)
  - Default values
- **ApplicationDbContext**: Main EF Core context
- Custom UUID converter for type-safe GUID handling
- Careful cascade behaviors for data integrity

#### DTOs (`/DTOs/`)
- **Role**: "Contract" for data transfer between application layers
- **Function**: Simple classes with properties but no business logic, used to:
  - Receive data from incoming requests (web forms, API calls)
  - Send data back in responses
  - Enforce validation rules using Data Annotations (`[Required]`, `[StringLength]`)
- Exclude sensitive information (passwords, internal IDs)

#### ViewModels (`/ViewModels/`)
- Models for binding data to Razor Views
- **AuthViewModel**: Login/Register forms with validation
- **ErrorViewModel**: Error page handling

### 3.4 Frontend Architecture

#### View Structure
- Uses shared layouts (`_Layout.cshtml`)
- **Component-based SCSS**: Organized in `/wwwroot/scss/components/`

#### Styling
- **Primary**: Tailwind CSS (via CDN) for layout and utility styling
- **UI Components**: Uses PrimeVue, managed locally via LibMan. All PrimeVue components are rendered within .cshtml files
- **Interactivity**: Vue.js (via LibMan) is used for progressive enhancement on specific components within .cshtml pages

#### Asset Pipeline
- **LibMan**: Client-side package management
- **SCSS Architecture**: Component-based organization

### 3.5 Architecture Flow Example
**Creating an Article - Request Flow:**

1. User submits a new article through the UI → **Controller** receives request
2. **Controller** receives request data and binds it to `CreateArticleDto`
3. **Controller** passes the `CreateArticleDto` to the `ArticleService`
4. **ArticleService** validates the DTO, creates an `Article` model instance, and populates it with DTO data
5. **ArticleService** uses the `DbContext` to save the `Article` model
6. Entity Framework Core reads the `ArticleConfiguration` to understand how to build the SQL `INSERT` statement and writes data to the database

This separation of concerns makes the application easier to maintain, test, and scale.

### 3.6 Database Schema
The database is designed based on the final SQL script provided, including tables for Users, Articles, Replies, PraiseCollect, Follows, Notifications, Reports, Hashtags, etc. The Users table includes a Role column to differentiate between members and administrators.

#### Security-First Approach
- UUID primary keys instead of sequential integers
- Foreign key constraints with appropriate cascade behaviors
- Default GUID values generated at database level

#### Entity Relationships
- **User-Person**: 1:1 relationship for profile separation
- **User-Article**: 1:many with cascade delete
- **Article-Reply**: Hierarchical comment system
- **User-User**: Friendship and Follow many-to-many relationships

### 3.7 Security Considerations

#### Current Implementation
- Basic password hashing is in place
- UUID primary keys for enhanced security
- Input validation through data annotations

#### Recommended Improvements
- Implement ASP.NET Core Identity for robust password management, session management, and CSRF protection
- Add proper session management or JWT authentication
- Implement comprehensive authorization policies

## 4. Development Patterns & Best Practices

### 4.1 Routing Convention
- Use explicit `[Route]` attributes for custom routes
- Both GET and POST methods should have matching routes for forms
- Example: `[Route("/login")]` for both GET and POST actions

### 4.2 Validation Strategy
- Data annotations on DTOs and ViewModels
- Model validation in controllers with `ModelState.IsValid`
- Business rule validation in Services layer

### 4.3 Error Handling
- Model validation errors through `ModelState.AddModelError`
- Custom error pages with ErrorViewModel
- Role-based redirection (Admin vs regular users)

### 4.4 Frontend Development Notes

#### Vue.js Integration
- Use Vue 3 Composition API with `setup()` function
- Mount Vue apps after DOM ready with jQuery
- Use `onMounted` lifecycle for DOM manipulation when template binding conflicts with Razor syntax
- Avoid `:class` binding conflicts by using programmatic class assignment in `onMounted`

#### SCSS Organization
- Component-based structure in `/wwwroot/scss/components/`
- Follow the brand color palette and component styling guidelines
- Tailwind CSS classes with `@apply` directive
- Layer-based organization with `@layer components`

#### Asset Compilation
- Tailwind CSS requires manual compilation via CLI
- SCSS files are compiled to `/wwwroot/css/`
- Use watch scripts (`tw.sh`/`tw.bat`) during development

### 4.5 Brand Compliance in Code
When implementing UI components, always ensure:
- All buttons use pill-shaped styling (`border-radius: 1000px`)
- Components use soft rounded corners (8px-12px)
- Color palette adherence to the defined HEX codes
- Consistent use of Lucide icons with line-style
- Text follows the brand tone guidelines