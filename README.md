# LearnHub - Online Learning Platform

A modern, feature-rich online learning platform built with ASP.NET Core 9 and Entity Framework Core.

## Features

### ?? Core Learning Features
- **Course Management**: Browse, search, and enroll in courses across multiple categories
- **Learning Paths**: Structured learning journeys with curated course sequences
- **Progress Tracking**: Track your learning progress across courses and lessons
- **Assessment System**: Skill assessment questionnaire for personalized recommendations
- **User Dashboard**: Personal learning dashboard with enrollments and progress

### ?? E-commerce Features
- **Order Management**: Complete order history and transaction tracking
- **Payment Integration**: Ready for payment gateway integration
- **Course Enrollment**: Automatic enrollment upon successful payment
- **Learning Path Purchases**: Buy complete learning paths at discounted prices

### ?? User Management
- **Authentication & Authorization**: Secure login/logout with cookie authentication
- **Role-based Access**: Different roles for Users, Instructors, and Admins
- **User Profiles**: Personalized user profiles and settings
- **Registration**: Easy user registration with validation

### ?? Modern UI/UX
- **Responsive Design**: Mobile-first responsive design
- **Modern Components**: Beautiful, accessible UI components
- **Dark/Light Theme Support**: Comprehensive design system
- **Performance Optimized**: Fast loading with optimized assets

## Technology Stack

- **Framework**: ASP.NET Core 9 with Razor Pages
- **Database**: Entity Framework Core with In-Memory Database
- **Authentication**: ASP.NET Core Identity with Cookie Authentication
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Architecture**: Clean Architecture with Service Layer pattern

## Quick Start

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Online-Learning-Platform-Ass1
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd OnlineLearningPlatformAss2.RazorWebApp
   dotnet run
   ```

5. **Open in browser**
   Navigate to `https://localhost:7049` or the URL shown in the terminal

### Default Test User
- **Username**: `testuser`
- **Email**: `test@example.com`
- **Password**: `password123`

## Project Structure

```
??? OnlineLearningPlatformAss2.RazorWebApp/     # Main web application
?   ??? Pages/                                   # Razor Pages
?   ??? wwwroot/                                # Static files (CSS, JS, images)
?   ??? Program.cs                              # Application entry point
??? OnlineLearningPlatformAss2.Service/        # Business logic layer
?   ??? Services/                               # Service implementations
?   ??? DTOs/                                   # Data Transfer Objects
?   ??? Interfaces/                             # Service interfaces
??? OnlineLearningPlatformAss2.Data/           # Data access layer
    ??? Database/                               # Entity Framework context
    ??? Entities/                               # Database entities
    ??? Configurations/                         # Entity configurations
```

## Key Features Guide

### 1. Course Browsing
- Visit the homepage to browse featured courses
- Use the search functionality to find specific courses
- Filter by categories and skill levels
- View detailed course information before enrolling

### 2. Skill Assessment
- Take the skill assessment from the navigation menu
- Get personalized course recommendations based on your answers
- Retake assessments to update recommendations

### 3. Learning Paths
- Browse curated learning paths for structured learning
- Enroll in complete paths for comprehensive skill development
- Track progress across multiple courses in a path

### 4. Order Management
- View all your orders in "My Orders" section
- Track payment status and transaction history
- Access order details and receipts

### 5. Learning Experience
- Access enrolled courses from "My Learning"
- Watch video lessons and track progress
- Complete modules in sequence

## Development Notes

### Database
- Uses Entity Framework In-Memory Database for development
- Database is automatically seeded with sample data on startup
- Sample data includes courses, learning paths, users, and orders

### Authentication
- Cookie-based authentication with 30-day expiration
- Role-based authorization (User, Instructor, Admin)
- Secure password hashing with BCrypt

### Performance
- Optimized asset loading with CDN resources
- Efficient database queries with EF Core
- Responsive design for fast mobile experience

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is for educational purposes. 

## Support

For issues or questions, please create an issue in the repository.

---

**LearnHub** - Empowering learning through technology ??
