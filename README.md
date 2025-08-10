Read this in: [繁體中文](README.zh-TW.md)

# Matrix
---

## Overview
Matrix is a sanctuary for Web3 pioneers and deep-tech enthusiasts, designed to filter out the noise of mainstream social media. We provide a pure, focused environment for high-quality discourse. Here, an on-chain credential is the only pass, ensuring a community built on expertise and shared consensus. While the platform may start with fragmented sparks of insight, we believe that as true peers connect, these unordered glimmers will form a grand and orderly matrix of thought.

## Tools
![ASP.NET](https://img.shields.io/badge/ASP.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white) ![DaisyUI](https://img.shields.io/badge/daisyui-5A0EF8?style=for-the-badge&logo=daisyui&logoColor=white) ![SCSS](https://img.shields.io/badge/SCSS-CC6699?style=for-the-badge&logo=sass&logoColor=white)


## Steps
1. You need to install NodeJS
2. Install sass
    ```
    npm i -g sass
    ```
3. You need to install libman if you don't have or doesn't use Visual Studio, please enter this command line to your terminal:
    ```
    dotnet tool install Microsoft.Web.LibraryManager.Cli
    npm i -g sass
    ```
4. You need to install the dependency packages into the `wwwroot/lib` folder.
    ```
    dotnet tool run libman restore
    ```
5. You also need to install these packages and tools.
    <i><b>If you are using Visual Studio, you can install it in Nuget Extensions Management.</b></i>
    ```
    dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 8.0.11
    dotnet add package MailKit
    dotnet add package AutoMapper
    ```
<br />
Because this project used DaisyUI UI Library, you need to install tailwindcss CLI and DaisyUI.<br>

  1. Get Tailwind CSS executable
  FollowTailwind CSS guideand get the latest version of Tailwind CSS executable for your OS.

      ###### windows
      ```
      curl -sLo tailwindcss.exe https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
      ```
      ###### MacOS
      ```
      curl -sLo tailwindcss https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-arm64
      ```
      Make the file executable (For Linux and MacOS): `chmod +x tailwindcss`

  1. Get daisyUI bundled JS file (already have)
  2. Watch CSS
      When you execute the following command, "tailwindcss" will be listened in the background.
      ###### MacOS
      ```
      sass "wwwroot/scss/main.scss" "wwwroot/css/components.css" -w --no-source-map
      ./tailwindcss -i "wwwroot/css/tailwind.css" -o "wwwroot/css/site.css" -w
      ```
      ###### windows
      ```
      sass ".\wwwroot\scss\main.scss" ".\wwwroot\css\components.css" -w --no-source-map
      .\tailwindcss.exe -i ".\wwwroot\css\tailwind.css" -o ".\wwwroot\css\site.css" -w
      ```

---
# P.S.

Check all EF tools infomations
```
dotnet list package
dotnet new view -n <cshtml name> -o <target folder>      # Create a new view 
```

---
## Architecture Explanation

This project follows a layered architecture pattern, primarily using the following folders to structure the code:

*   `Controllers`
*   `Models`
*   `Data/Configurations`
*   `DTOs` (Data Transfer Objects)
*   `Services`

Here is an explanation of their roles and relationships:

### 1. `Controllers`

*   **Role**: The **Controllers** act as the entry points for incoming requests (e.g., from a web browser or an API client). They are responsible for handling user input and orchestrating the response.
*   **Function**: A controller receives a request, calls the appropriate `Service` to perform the necessary business logic, and then returns a suitable response (e.g., a web page, JSON data, or a redirect).
*   **Key Principle**: Controllers should be thin. They should not contain business logic or directly interact with the database. Their primary role is to coordinate between the view/client and the service layer.

### 2. `Models`

*   **Role**: The **Models** are the "blueprints" of the data in your application. They define the shape and relationships of your data in the C# world.
*   **Function**: A model class (e.g., `Article.cs`) defines the properties of an entity and its relationships with other entities. It's like a C# representation of a database table.

### 3. `Data/Configurations`

*   **Role**: This layer acts as the "construction manual" for the database. It defines how the `Models` are mapped to the actual database tables.
*   **Function**: Using the Fluent API, these files provide detailed instructions to Entity Framework Core on how to build the database schema. This includes setting up primary keys, defining column properties (like max length), creating indexes, and establishing default values.

### 4. `DTOs` (Data Transfer Objects)

*   **Role**: DTOs act as a "contract" or a defined shape for data that is transferred between different layers of the application, especially between the **Controllers** and the **Services**.
*   **Function**: They are simple classes that contain properties but no business logic. They are used to:
    *   Receive data from incoming requests (e.g., from a web form or an API call).
    *   Send data back in responses.
    *   Enforce validation rules using Data Annotations (e.g., `[Required]`, `[StringLength]`).

### 5. `Services`

*   **Role**: The **Services** layer is the "brain" of the application. It contains the core business logic.
*   **Function**: A service class takes a DTO from the Controller, processes it, and performs the necessary actions. This can include:
    *   Validating the data based on business rules.
    *   Interacting with the database (via the `DbContext`).
    *   Calling other services.
    *   Mapping data from a DTO to a database model (Entity) and vice versa.

### Relationship Flow (Example: Creating an Article)

1.  A user submits a new article through the UI. The request hits a **Controller**.
2.  The **Controller** receives the request data and binds it to a `CreateArticleDto`.
3.  The **Controller** passes the `CreateArticleDto` to the `ArticleService`.
4.  The `ArticleService` validates the DTO, creates an `Article` model instance, and populates it with the DTO's data.
5.  The `ArticleService` uses the `DbContext` to save the `Article` model.
6.  Entity Framework Core reads the `ArticleConfiguration` to understand how to build the SQL `INSERT` statement and writes the data to the database.

This separation of concerns makes the application easier to maintain, test, and scale.

---

## Code First Guidelines & Best Practices

This project follows the **Entity Framework Core Code First** approach, which means the database schema is generated from the code (Models). To maintain code integrity and avoid synchronization issues, please follow these guidelines:

### 🚨 **NEVER** modify the database directly!

**ALWAYS** use the Code First workflow when making database changes:

### Correct Workflow for Database Changes

1. **Modify the Model** first in the `/Models/` folder
2. **Update the Configuration** if needed in `/Data/Configurations/`
3. **Create a Migration** using EF Core tools
4. **Apply the Migration** to update the database

### Essential Commands

```bash
# Add a new migration after model changes
dotnet ef migrations add <MigrationName>

# Apply pending migrations to database
dotnet ef database update

# Remove the last migration (if not yet applied)
dotnet ef migrations remove

# Check migration status
dotnet ef migrations list

# Generate SQL script from migrations
dotnet ef migrations script
```

### Step-by-Step Example: Adding a New Field

1. **Add the field to your Model:**
   ```csharp
   // In Models/Person.cs
   [MaxLength(100)]
   public string? NewField { get; set; }
   ```

2. **Update Configuration (if needed):**
   ```csharp
   // In Data/Configurations/PersonConfiguration.cs
   builder.Property(p => p.NewField)
       .HasMaxLength(100);
   ```

3. **Create Migration:**
   ```bash
   dotnet ef migrations add AddNewFieldToPerson
   ```

4. **Apply Migration:**
   ```bash
   dotnet ef database update
   ```

### Common Scenarios & Solutions

#### 🔄 **If someone accidentally modified the database directly:**

1. Manually add the missing fields to the appropriate Model
2. Update the Configuration if needed
3. **Delete the manually added columns from the database**
4. Create a new migration: `dotnet ef migrations add RestoreCodeFirstIntegrity`
5. Apply the migration: `dotnet ef database update`

#### 🔍 **Checking for synchronization issues:**
```bash
# This will create an empty migration if everything is in sync
dotnet ef migrations add CheckSync

# If the migration is empty, remove it
dotnet ef migrations remove
```

#### 📝 **Migration naming conventions:**
- Use descriptive names: `AddUserEmailField`, `UpdateArticleConstraints`
- Use PascalCase format
- Include the action and affected entity

### Database Connection

The project uses Entity Framework Core with SQL Server. Connection string should be configured in:
- `appsettings.json` for production
- `appsettings.Development.json` for development
- Or use `.env` file (with DotNetEnv package)

### ⚠️ Important Notes

- **Never** run direct SQL commands on the database for schema changes
- **Always** test migrations on a development database first
- **Backup** your database before applying migrations in production
- **Review** generated migration code before applying
- **Keep** migration files in version control

Following these guidelines ensures that your database schema stays in sync with your code and prevents data loss or corruption issues.

---

## Backend–Frontend Integration (Same-Origin vs CORS)

- Same-origin by default: When Vue runs from `wwwroot/js` and renders CSHTML in the same ASP.NET app, requests are same-origin and do not require CORS. Global CORS has been removed accordingly.
- Cross-origin (future option): If you later serve the frontend from another origin (e.g., a separate dev server), use a named CORS policy with `WithOrigins(...) + AllowCredentials()`, and make sure the exact origin (scheme + host + port) matches. On the frontend set `withCredentials` for Axios/fetch.

## Data Protection Keys Persistence

- Purpose: Avoid 401/403 after restarts by persisting ASP.NET Data Protection keys used for cookies and antiforgery.
- Location: Keys are written to `DataProtectionKeys/` at runtime and ignored by Git (`DataProtectionKeys/*.xml`). A placeholder `.gitkeep` is included so the folder is visible.
- Change location (optional): You can move keys to a user/machine path, e.g.
  - `var keysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Matrix", "Keys");`

### What the `DataProtectionKeys/` folder does

- Stores XML key material for ASP.NET Core Data Protection, which encrypts/decrypts:
  - Authentication cookies (incl. identity/JWT-in-cookie scenarios)
  - Antiforgery tokens
  - TempData cookie (when cookie-based)
- Created at app startup; the first time protection is needed, a key ring file is written.
- Safe to delete in development, but note the impact:
  - Existing auth cookies and antiforgery tokens become invalid; users must sign in again.
  - This is usually fine in dev; avoid deleting in production.
- Production guidance:
  - Keep keys persistent across restarts/deployments; back them up if appropriate.
  - For multi-instance deployments, use a shared location so all instances use the same keys.
  - Ensure consistent ApplicationName (this repo sets `.SetApplicationName("Matrix")`).
- Permissions: the app process must be able to create and write files in the chosen directory.

## Cookie Mode Toggle (Auth:CrossSiteCookies)

- Config path: `appsettings.Development.json` → `Auth:CrossSiteCookies`.
- Same-origin HTTP (default): `false` → Cookies use `SameSite=Lax; Secure=false`.
- Cross-origin over HTTPS: `true` → Cookies use `SameSite=None; Secure=true` and require HTTPS; set frontend requests to send credentials.

## Notes on Ports and 403

- Port matching only matters for cross-origin scenarios. In same-origin, no CORS matching is needed.
- Persisted keys reduce auth/antiforgery failures after `dotnet run` restarts. If you still see 403 after a restart, refresh the page or use `dotnet watch run` to avoid full restarts.
