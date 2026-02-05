# OnlineLearningPlatformAss2 - Razor Pages Edition

An ASP.NET Core Razor Pages web application for online learning management, converted from the original MVC architecture.

## ?? Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Razor Pages** - Server-side rendering web framework
- **C#** - Primary programming language
- **Entity Framework Core** - Data access and ORM
- **SQL Server** - Database
- **Bootstrap** - Frontend CSS framework
- **jQuery** - JavaScript library
- **SignalR** - Real-time communication

## ?? Project Structure

```
OnlineLearningPlatformAss2/
??? OnlineLearningPlatformAss2.RazorWebApp/     # Main Razor Pages application
?   ??? Pages/                                   # Razor Pages
?   ?   ??? User/                               # User management pages
?   ?   ??? Course/                             # Course-related pages
?   ?   ??? Assessment/                         # Assessment pages
?   ?   ??? Shared/                             # Shared layouts and partials
?   ??? Models/                                 # Page models and view models
?   ??? Hubs/                                   # SignalR hubs
?   ??? wwwroot/                                # Static files (CSS, JS, images)
?   ??? Program.cs                              # Application entry point
??? OnlineLearningPlatformAss2.Service/         # Business logic layer
?   ??? Services/                               # Service implementations
?   ??? DTOs/                                   # Data transfer objects
?   ??? Results/                                # Service result patterns
??? OnlineLearningPlatformAss2.Data/            # Data access layer
?   ??? Database/                               # EF Core DbContext and entities
?   ??? Repositories/                           # Repository implementations
??? OnlineLearningPlatformAss2.sln              # Solution file
```

## ??? Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- A code editor (Visual Studio, VS Code, or Rider)
- SQL Server (LocalDB or full SQL Server)

## ?? Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd OnlineLearningPlatformAss2
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   Edit `OnlineLearningPlatformAss2.RazorWebApp/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

4. **Create/Update database**
   ```bash
   dotnet ef database update --project OnlineLearningPlatformAss2.Data --startup-project OnlineLearningPlatformAss2.RazorWebApp
   ```

5. **Run the application**
   ```bash
   cd OnlineLearningPlatformAss2.RazorWebApp
   dotnet run
   ```

## ??? Architecture

The project follows a **3-tier layered architecture**:

### Presentation Layer (RazorWebApp)
- **Razor Pages**: Server-side rendered pages with code-behind models
- **Page Models**: Handle HTTP requests and business logic coordination
- **ViewModels**: Data structures for rendering views
- **Static Assets**: CSS, JavaScript, and images

### Business Logic Layer (Service)
- **Services**: Business logic implementation
- **DTOs**: Data transfer objects for API communication
- **Validators**: Input validation using FluentValidation
- **Service Results**: Standardized response patterns

### Data Access Layer (Data)
- **DbContext**: Entity Framework Core database context
- **Entities**: Domain models representing database tables
- **Repositories**: Data access abstraction layer
- **Configurations**: EF Core entity configurations

## ?? Differences from MVC Version

| Aspect | MVC | Razor Pages |
|--------|-----|-------------|
| **Structure** | Controller + Action Methods | Page + PageModel |
| **Routing** | Controller/Action based | Page-based routing |
| **Organization** | Separate Controllers/Views | Co-located Pages |
| **URL Structure** | `/Controller/Action` | `/PageName` |
| **File Organization** | Controllers, Views, Models folders | Pages with code-behind |
