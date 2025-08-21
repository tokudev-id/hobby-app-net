# HobbyApp - .NET 8 User Management System

A modern simple .NET 8 web application for comprehensive user management with hobby tracking, role-based access control, and a beautiful responsive UI.

## ✨ Key Features

### **Authentication & Authorization**
- **JWT Authentication** with HttpOnly cookies for security
- **Role-Based Access Control** (Admin/User roles)
- **Automatic token refresh** for seamless user experience
- **Protected routes** and API endpoints

### **User Management**
- **Complete CRUD operations** for users
- **User profile management** with hobby tracking
- **Role assignment** and management
- **User search and filtering**
- **Pagination** for large datasets

### **Hobby System**
- **Multi-hobby support** per user
- **Skill levels** (Beginner, Intermediate, Expert)
- **Duplicate prevention** with case-sensitive validation
- **Dynamic hobby management** in forms

### **Modern UI/UX**
- **Responsive design** with modern CSS
- **Card and List view** toggle for users
- **Real-time form validation** with visual feedback
- **Toast notifications** for user actions
- **Password visibility toggle** for better UX
- **Search with clear button** and debounced input

## Architecture

Simplified Clean Architecture with a single-project structure:

```
src/
├── Domain/
│   └── Entities/           # User, Hobby, Role, UserRole
├── Application/
│   ├── DTOs/               # Data Transfer Objects
│   ├── Services/           # Business logic services
│   └── Mapping/            # AutoMapper profiles
├── Infrastructure/
│   ├── Persistence/        # EF Core & database
│   ├── Repositories/       # Repository pattern
│   ├── Security/           # JWT services
│   └── Middleware/         # Custom middleware
├── Controllers/
│   ├── Api/                # API controllers
│   └── Pages/              # Page controllers
├── Views/                  # Razor views
└── wwwroot/               # Static assets
```

## 🛠️ Tech Stack

- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core MVC** - Web framework with Razor Pages
- **Entity Framework Core** - ORM with Code First migrations
- **MySQL** - Database with Pomelo provider
- **JWT Authentication** - Secure token-based auth
- **AutoMapper** - Object mapping
- **Modern CSS** - Custom responsive design
- **JavaScript** - Enhanced client-side interactions

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- MySQL Server 8.0+
- Your favorite IDE (VS Code, Visual Studio, Rider)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd HobbyApp
   ```

2. **Configure database**
   ```bash
   # Update connection string in appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;database=hobbyapp_db;user=root;password=yourpassword"
     }
   }
   ```

3. **Install and run**
   ```bash
   dotnet restore
   dotnet run
   ```

4. **Access the application**
   - Web UI: `https://localhost:5263`
   - Default admin: `admin@hobbyapp.com` / `Admin123!`

### Database Auto-Migration
The application automatically:
- Creates the database on first run
- Applies migrations
- Seeds initial data (admin user and roles)

### Troubleshooting Database Issues

If you encounter migration errors on first run:

```bash
# Option 1: Reset database completely
dotnet ef database drop --force
dotnet run

# Option 2: Manual database reset (MySQL)
mysql -u root -p
DROP DATABASE IF EXISTS hobbyapp_db;
CREATE DATABASE hobbyapp_db;
exit
dotnet run
```

## 🔑 Default Accounts

| Role  | Username | Email                | Password   |
|-------|----------|---------------------|------------|
| Admin | admin    | admin@hobbyapp.com  | Admin123!  |
| User  | asep_wijaya     | asep.wijaya@example.com   | Password123!   |

## 📱 Features in Detail

### **User Interface**
- **Modern Design**: Clean, professional look with CSS variables
- **Responsive**: Works perfectly on desktop, tablet, and mobile
- **Accessibility**: Proper ARIA labels and keyboard navigation

### **User Management**
- **List View**: Sortable table with search and pagination
- **Card View**: Visual cards with user avatars and quick actions
- **User Details**: Comprehensive profile view
- **Form Validation**: Real-time client and server-side validation

### **Security Features**
- **HttpOnly Cookies**: Secure token storage
- **CSRF Protection**: Anti-forgery tokens
- **Role-based UI**: Dynamic content based on user permissions
- **Input Sanitization**: XSS protection

### **Admin Features**
- **User Management**: Create, edit, delete users
- **Role Assignment**: Assign/remove roles from users

### **User Features**
- **Profile Management**: Edit own profile and hobbies
- **Hobby Tracking**: Add/remove hobbies with skill levels

---

**Built with ❤️ using .NET 8 and modern web technologies**