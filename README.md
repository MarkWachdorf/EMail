# Email Microservice

A comprehensive ASP.NET Core microservice for sending, caching, and managing email messages.

## 🚀 Quick Start

### Running the API with Swagger

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Run the API:**
   
   **Option A - Using run scripts (recommended):**
   ```bash
   # PowerShell
   .\run-api.ps1
   
   # Windows Batch
   run-api.bat
   ```
   
   **Option B - Direct command:**
   ```bash
   dotnet run --project Email.API/Email.API.csproj
   ```

3. **Access Swagger UI:**
   - The browser will automatically open to: `https://localhost:7198/swagger`
   - Or manually navigate to: `https://localhost:7198/swagger`

### API Endpoints

The API provides the following main controllers:

- **EmailController** - Manage email messages (CRUD operations)
- **EmailSenderController** - Send emails and process pending emails
- **EmailCacheController** - Handle cached email consolidation
- **ErrorLogController** - Manage error logs

### Testing

Run all tests:
```bash
dotnet test
```

Run only unit tests:
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

## 📋 Features

- ✅ Email message management (create, read, update, delete)
- ✅ Email sending with retry logic
- ✅ Email caching for consolidation
- ✅ Error logging and monitoring
- ✅ Comprehensive API documentation with Swagger
- ✅ Unit and integration tests
- ✅ Clean architecture with separation of concerns

## 🏗️ Architecture

- **Email.API** - ASP.NET Core Web API with controllers
- **Email.Application** - Business logic and services
- **Email.Infrastructure** - Data access and external integrations
- **Email.Contracts** - DTOs and shared models
- **Email.Tests** - Unit and integration tests