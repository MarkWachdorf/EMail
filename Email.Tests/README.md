# Email Microservice Testing

This directory contains comprehensive tests for the Email Microservice, including both unit tests and integration tests.

## Test Structure

```
Email.Tests/
├── Unit/
│   ├── Services/
│   │   ├── EmailServiceTests.cs
│   │   ├── EmailSenderServiceTests.cs
│   │   └── EmailCacheServiceTests.cs
│   └── Controllers/
│       └── EmailControllerTests.cs
├── Integration/
│   ├── EmailIntegrationTestFixture.cs
│   └── EmailFlowIntegrationTests.cs
├── run-tests.ps1
└── README.md
```

## Test Categories

### Unit Tests
- **Service Layer Tests**: Test business logic in isolation using mocked dependencies
- **Controller Tests**: Test API endpoints with mocked services
- **Repository Tests**: Test data access patterns (if needed)

### Integration Tests
- **End-to-End Flow Tests**: Test complete email workflows from API to database
- **Cache Consolidation Tests**: Test email caching and consolidation scenarios
- **Error Handling Tests**: Test error scenarios and recovery mechanisms

## Prerequisites

1. **.NET 8.0 SDK** installed
2. **SQL Server LocalDB** (for integration tests)
3. **Test Database**: The integration tests will create a test database automatically

## Running Tests

### Option 1: Using the Test Runner Script (Recommended)

```powershell
# Navigate to the test directory
cd Email.Tests

# Run the test runner script
.\run-tests.ps1
```

### Option 2: Using dotnet CLI

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run with detailed output
dotnet test --verbosity normal --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~EmailServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~CreateEmailAsync_ValidRequest_ShouldCreateEmailAndLogHistory"
```

### Option 3: Using Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Run tests from the Test Explorer window

## Test Configuration

### Environment Variables
The tests use the following environment variables:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Test"
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\mssqllocaldb;Database=EmailTestDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

### Test Database
- **Database Name**: `EmailTestDb`
- **Server**: LocalDB
- **Connection**: Trusted Connection
- **Auto-Creation**: The test fixture will create the database if it doesn't exist

## Test Data

### Email Addresses
All test emails are configured to send to: **wachdorfm@hotmail.com**

### Test Company Codes
- `TEST` - General test emails
- `INTEGRATION` - Integration test emails
- `CACHE` - Cache consolidation test emails
- `RETRY` - Retry mechanism test emails
- `STATUS` - Status update test emails

## Test Scenarios

### Unit Test Scenarios

#### EmailService Tests
- ✅ Create email with valid request
- ✅ Get email by ID (existing and non-existing)
- ✅ Get all emails with filtering and pagination
- ✅ Update email status with optimistic concurrency
- ✅ Soft delete email
- ✅ Handle concurrency conflicts
- ✅ Handle not found scenarios

#### EmailSenderService Tests
- ✅ Send email immediately
- ✅ Process pending emails in batch
- ✅ Retry failed emails
- ✅ Handle retry limits
- ✅ Handle email not found scenarios
- ✅ Handle invalid status scenarios

#### EmailCacheService Tests
- ✅ Create new cache entry
- ✅ Update existing cache entry
- ✅ Handle expired cache entries
- ✅ Process expired cache entries
- ✅ Send consolidated emails
- ✅ Handle sending failures

#### Controller Tests
- ✅ All CRUD operations
- ✅ Proper HTTP status codes
- ✅ Error handling and validation
- ✅ Pagination and filtering
- ✅ Optimistic concurrency handling

### Integration Test Scenarios

#### Complete Email Flow
1. Create email via API
2. Retrieve created email
3. Send email immediately
4. Process pending emails
5. Verify email status changes
6. Test pagination and filtering

#### Cache Consolidation Flow
1. Send first cached email
2. Send second cached email (should consolidate)
3. Process expired cache entries
4. Verify consolidated email is sent

#### Error Handling Flow
1. Test invalid request validation
2. Test missing required fields
3. Test invalid email formats
4. Test retry mechanisms

#### Status Update Flow
1. Create email
2. Update email status
3. Verify status change
4. Test optimistic concurrency

## Test Dependencies

### NuGet Packages
- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for tests

### Project References
- `Email.Application` - Business logic layer
- `Email.API` - API layer
- `Email.Contracts` - DTOs and contracts

## Test Best Practices

### Unit Tests
- ✅ Use mocking for external dependencies
- ✅ Test one scenario per test method
- ✅ Use descriptive test names
- ✅ Follow AAA pattern (Arrange, Act, Assert)
- ✅ Test both success and failure scenarios
- ✅ Test edge cases and boundary conditions

### Integration Tests
- ✅ Use real database (LocalDB)
- ✅ Test complete workflows
- ✅ Clean up test data
- ✅ Use unique test data to avoid conflicts
- ✅ Test error scenarios end-to-end

### General
- ✅ Use meaningful test data
- ✅ Avoid test interdependencies
- ✅ Use proper assertions
- ✅ Handle async operations correctly
- ✅ Use appropriate test categories

## Troubleshooting

### Common Issues

#### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

#### Database Connection Issues
```bash
# Ensure LocalDB is running
sqllocaldb start "MSSQLLocalDB"

# Create test database manually if needed
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE EmailTestDb"
```

#### Test Execution Issues
```bash
# Run with detailed logging
dotnet test --verbosity detailed --logger "console;verbosity=detailed"

# Run specific test to isolate issues
dotnet test --filter "FullyQualifiedName~SpecificTestName"
```

### Performance Tips
- Run unit tests frequently during development
- Run integration tests before commits
- Use test parallelization for faster execution
- Consider using test data builders for complex scenarios

## Continuous Integration

The tests are designed to work in CI/CD pipelines:

```yaml
# Example GitHub Actions step
- name: Run Tests
  run: |
    dotnet test Email.Tests/Email.Tests.csproj --configuration Release --verbosity normal --logger "console;verbosity=detailed"
```

## Coverage

The tests cover:
- ✅ All service methods
- ✅ All controller endpoints
- ✅ All business logic scenarios
- ✅ Error handling and edge cases
- ✅ Integration workflows
- ✅ Data validation
- ✅ Optimistic concurrency

## Contributing

When adding new features:
1. Write unit tests for the service layer
2. Write unit tests for the controller layer
3. Write integration tests for end-to-end scenarios
4. Update this README if needed
5. Ensure all tests pass before submitting

## Support

For test-related issues:
1. Check the troubleshooting section
2. Review test logs for detailed error information
3. Ensure all prerequisites are met
4. Verify test data and configuration
