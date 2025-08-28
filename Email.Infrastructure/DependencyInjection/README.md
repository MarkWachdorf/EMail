# Email Infrastructure Dependency Injection

This document explains how to set up and use the dependency injection for the Email Infrastructure services.

## Overview

The infrastructure layer provides dependency injection setup for:
- Database connection factory
- Repository implementations
- Configuration options

## Quick Start

### Basic Setup

```csharp
// In Program.cs or Startup.cs
using Email.Infrastructure.DependencyInjection;

// Add infrastructure services with default configuration
builder.Services.AddInfrastructureServices(builder.Configuration);
```

### Advanced Setup with Custom Options

```csharp
// Add infrastructure services with custom configuration
builder.Services.AddInfrastructureServices(builder.Configuration, options =>
{
    options.ConnectionStringName = "EmailDatabase";
    options.EnableRetryPolicy = true;
    options.MaxRetryAttempts = 5;
    options.RetryDelaySeconds = 2;
    options.EnableConnectionPooling = true;
    options.MaxPoolSize = 200;
    options.MinPoolSize = 10;
    options.ConnectionTimeout = 60;
    options.CommandTimeout = 60;
});
```

### Custom Repository Lifetime

```csharp
// Register repositories with custom lifetime (e.g., Singleton for testing)
builder.Services.AddInfrastructureServices(
    builder.Configuration, 
    options => { /* configuration */ }, 
    ServiceLifetime.Singleton);
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmailMicroservice;Trusted_Connection=true;"
  },
  "Infrastructure": {
    "ConnectionStringName": "DefaultConnection",
    "EnableRetryPolicy": true,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 1,
    "EnableConnectionPooling": true,
    "MaxPoolSize": 100,
    "MinPoolSize": 0,
    "ConnectionTimeout": 30,
    "CommandTimeout": 30
  }
}
```

## Registered Services

### Connection Factory
- **Interface**: `IDbConnectionFactory`
- **Implementation**: `SqlConnectionFactory`
- **Lifetime**: Singleton
- **Purpose**: Creates and manages database connections

### Repositories
- **Interface**: `IEmailMessageRepository`
- **Implementation**: `EmailMessageRepository`
- **Lifetime**: Scoped (default)
- **Purpose**: Email message CRUD operations

- **Interface**: `IEmailCacheRepository`
- **Implementation**: `EmailCacheRepository`
- **Lifetime**: Scoped (default)
- **Purpose**: Email caching operations

- **Interface**: `IErrorLogRepository`
- **Implementation**: `ErrorLogRepository`
- **Lifetime**: Scoped (default)
- **Purpose**: Error logging operations

- **Interface**: `IEmailHistoryRepository`
- **Implementation**: `EmailHistoryRepository`
- **Lifetime**: Scoped (default)
- **Purpose**: Email history tracking

## Usage in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailMessageRepository _emailRepository;
    private readonly IErrorLogRepository _errorRepository;

    public EmailController(
        IEmailMessageRepository emailRepository,
        IErrorLogRepository errorRepository)
    {
        _emailRepository = emailRepository;
        _errorRepository = errorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmails()
    {
        var emails = await _emailRepository.GetAllAsync();
        return Ok(emails);
    }
}
```

## Testing

For unit testing, you can register repositories with `ServiceLifetime.Singleton`:

```csharp
// In test setup
services.AddInfrastructureServices(
    configuration, 
    options => { /* test configuration */ }, 
    ServiceLifetime.Singleton);
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| ConnectionStringName | string | "DefaultConnection" | Name of the connection string in configuration |
| EnableRetryPolicy | bool | true | Whether to enable retry policy for database operations |
| MaxRetryAttempts | int | 3 | Maximum number of retry attempts |
| RetryDelaySeconds | int | 1 | Delay between retry attempts in seconds |
| EnableConnectionPooling | bool | true | Whether to enable connection pooling |
| MaxPoolSize | int | 100 | Maximum pool size for connection pooling |
| MinPoolSize | int | 0 | Minimum pool size for connection pooling |
| ConnectionTimeout | int | 30 | Connection timeout in seconds |
| CommandTimeout | int | 30 | Command timeout in seconds |

## Best Practices

1. **Use Scoped Lifetime**: Repositories should typically be registered with `Scoped` lifetime for web applications
2. **Configure Connection String**: Always use a named connection string in configuration
3. **Enable Connection Pooling**: Keep connection pooling enabled for better performance
4. **Set Appropriate Timeouts**: Configure timeouts based on your application requirements
5. **Use Retry Policy**: Enable retry policy for resilience against transient failures
