# HobbyApp - .NET 8 User Management System

A modern .NET 8 web application for user management with hobby tracking and role-based access control.

## 🚀 Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/tokudev-id/hobby-app-net.git
cd hobby-app-net
```

### 2. Prerequisites
- .NET 8.0 SDK
- MySQL Server 8.0+

### 3. Configure Database
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=hobbyapp_db;user=root;password=yourpassword"
  }
}
```

### 4. Run Application
```bash
dotnet restore
dotnet run
```

### 5. Access Application
- **URL**: `https://localhost:5263`
- **Admin Login**: `admin@hobbyapp.com` / `Admin123!`
- **User Login**: `asep.wijaya@example.com` / `Password123!`

## ✨ Key Features

- **JWT Authentication** with role-based access control
- **User Management** with CRUD operations and search
- **Hobby System** with skill levels and validation
- **Modern UI** with responsive design and real-time validation
- **Role Management** for administrators
- **Auto Database Migration** and seeding

## 🛠️ Tech Stack

- .NET 8.0 + ASP.NET Core MVC
- Entity Framework Core + MySQL
- JWT Authentication + FluentValidation
- Modern CSS + JavaScript

## 🔧 Database Auto-Setup

The application automatically:
- Creates database on first run
- Applies migrations
- Seeds admin user and default roles

## 🚨 Troubleshooting

If you encounter database migration errors:

```bash
# Reset database
dotnet ef database drop --force
dotnet run
```

Or manually reset MySQL:
```bash
mysql -u root -p
DROP DATABASE IF EXISTS hobbyapp_db;
CREATE DATABASE hobbyapp_db;
exit
dotnet run
```

## 📚 Documentation

For detailed feature documentation, screenshots, and technical implementation details, see:
**[📖 Complete Documentation](Docs/doc.md)**

## 🔑 Default Accounts

| Role  | Email                   | Password     |
|-------|------------------------|--------------|
| Admin | admin@hobbyapp.com     | Admin123!    |
| User  | asep.wijaya@example.com| Password123! |

---

**Built with ❤️ using .NET 8 and modern web technologies**