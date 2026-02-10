# Recruitment API

A comprehensive recruitment management system built with .NET 10, following Clean Architecture and Domain-Driven Design principles.

## Quick Start

### Prerequisites
- .NET 10 SDK (Preview 7 or later)
- Visual Studio 2024 or Visual Studio Code

### Running the Application

1. **Clone and Build**
   ```bash
   cd CSharpSolutions
   dotnet build
   ```

2. **Run the API**
   ```bash
   cd Recruiter/WebApi
   dotnet run
   ```

3. **Test the Sample Endpoints**
   
   The application will start on:
   - HTTP: `http://localhost:5140`
   - HTTPS: `https://localhost:7168`

   **Available Sample Endpoints:**
   - **GET** `/weatherforecast` - Default minimal API endpoint

## 📁 Project Structure

```
CSharpSolutions/
└── Recruiter/
    ├── Domain/                     # Core domain models and business logic
    ├── Infrastructure/             # Data access and external services
    ├── Application/               # Use cases and application services
    ├── WebApi/                   # API controllers and presentation layer
    ├── UnitTests/               # Unit and integration tests
    └── CVProcessor/             # Microservice for CV processing
```

## 🏗️ Architecture

- **Clean Architecture** - Proper separation of concerns
- **Domain-Driven Design** - Rich domain models and aggregates
- **.NET 10** - Latest framework features
- **SOLID Principles** - Maintainable and testable code


## 🧪 Testing

Run all tests:
```bash
dotnet test
```

## 📝 API Documentation

When running in Development mode, the API documentation is available at:
- OpenAPI: `/openapi/v1.json`

---

**Framework:** .NET 10.0.100-preview.7  
**Architecture:** Clean Architecture + DDD  
**Status:** ✅ Foundation Complete - Ready for Domain Implementation
