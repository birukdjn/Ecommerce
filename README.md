# Multi-Vendor E-Commerce API

A scalable and maintainable backend system for a multi-vendor marketplace, built using modern .NET technologies and clean software architecture principles.

---

## 🛠️ Tech Stack

### Core
- **Framework:** ASP.NET Core (.NET 10)
- **Language:** C# 14
- **Architecture:** Clean Architecture (Layered + CQRS)
- **Mediator Pattern:** MediatR 12
  
### Data & Persistence
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Patterns:** Generic Repository + Unit of Work
- **Migrations:** EF Core CLI

### Infrastructure
- **Authentication:** ASP.NET Core Identity
- **Authorization:** Role-Based Access Control (RBAC)
- **Background Jobs:** Hangfire
- **Containerization:** Docker

### API & Testing
- **API Docs:** Swagger / OpenAPI
- **Testing Tools:** Postman

---

## 🧱 Architecture Overview

The project follows **Clean Architecture** principles:

- **Domain Layer:** Core business logic and entities
- **Application Layer:** Use cases, DTOs, interfaces, validation
- **Infrastructure Layer:** Database, external services, background jobs
- **Presentation Layer:** API controllers and endpoints

Key design patterns:
- CQRS (Command Query Responsibility Segregation)
- Dependency Injection
- Separation of Concerns

---

## 🚀 Features

### 1. Multi-Vendor Support
- Vendors manage their own products
- Orders can contain items from multiple vendors
- Vendor-specific order tracking

### 2. Order & Inventory Management
- Automatic stock reservation on order creation
- Background job releases stock for unpaid orders
- Prevents inventory inconsistencies

### 3. Background Processing
- Scheduled and recurring jobs using Hangfire:
  - Order expiration handling
  - Stock synchronization
  - Notification triggers

### 4. Secure Authentication & Authorization
- Identity class Authentication
- Role-based access (Admin, Vendor, Customer)
- Secure password hashing and validation

---

## 🔒 Best Practices Implemented

### API Design
- RESTful API conventions
- Proper HTTP status codes


### Validation & Error Handling
- Centralized validation using FluentValidation
- Global exception handling middleware
- Standardized API response format

### Performance & Scalability
- Asynchronous programming (async/await)
- Pagination for large datasets
- Optimized queries with EF Core
- Caching strategy 

### Security
- Input validation and sanitization
- token protection
- HTTPS enforcement
- Rate limiting

### Logging & Monitoring
- Structured logging
- Request/response logging
- Health checks endpoint

---

## ⚙️ Setup & Installation

### 1. Clone the Repository
```bash
git clone https://github.com/birukdjn/ecommerce.git
cd ecommerce
```

### 2. Configure Environment
Update `appsettings.json` or use environment variables:

- Database connection string  
- External service keys

### 3. Apply Database Migrations
```bash
dotnet ef database update
```

### 4. Run Locally (Options)
Option 1: Using Docker
```bash
docker-compose up --build
```
Option 1: Using dotnet run
```bash
dotnet restore
dotnet build
dotnet run --project Api
```
## 📂 Project Structure

```bash
src/
├── Domain/
│   ├── Common/
│   ├── Constants/
│   ├── Entities/
│   └── Enums/
│
├── Application/
│   ├── DTOs/
│   │   ├── Address/
│   │   ├── Admin/
│   │   ├── ...
│   │   ├── ...
│   │   └── Wallet/
│   ├── Features/
│   │   ├── Products/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Orders/
│   │   ├── Users/
│   │   ├── ...
│   │   ├── ...
│   │   └── Wallets/
│   ├── Interfaces/
│   ├── Templates
│   │   ├── Email/
│   └── DependencyInjection.cs
│
├── Infrastructure/
│   ├── BackgroundJobs/
│   ├── Configurations/
│   ├── Context/
│   ├── Extensions/
│   ├── Identity/
│   ├── Migrations/
│   ├── Options/
│   ├── Repositories/
│   ├── Services/
│   └── DependencyInjection.cs
│
├── API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   ├── Extensions/
│   ├── Properties/
│   ├── DependencyInjection.cs
│   ├── Dockerfile
│   ├── appsettings.json
│   └── Program.cs
```
## 👨‍💻 Author

**Biruk Dejene**

- LinkedIn: https://linkedin.com/in/birukdjn  
- Portfolio: https://birukdjn.vercel.app  
