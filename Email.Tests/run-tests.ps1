# Email Microservice Test Runner
# This script runs all unit and integration tests for the email microservice

Write-Host "Starting Email Microservice Tests..." -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Set test environment variables
$env:ASPNETCORE_ENVIRONMENT = "Test"
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\mssqllocaldb;Database=EmailTestDb;Trusted_Connection=true;MultipleActiveResultSets=true"

# Build the test project
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build Email.Tests/Email.Tests.csproj --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run unit tests first
Write-Host "Running Unit Tests..." -ForegroundColor Yellow
Write-Host "=====================" -ForegroundColor Yellow
dotnet test Email.Tests/Email.Tests.csproj --filter "Category=Unit" --configuration Release --verbosity normal --logger "console;verbosity=detailed"

$unitTestExitCode = $LASTEXITCODE

# Run integration tests
Write-Host "Running Integration Tests..." -ForegroundColor Yellow
Write-Host "============================" -ForegroundColor Yellow
dotnet test Email.Tests/Email.Tests.csproj --filter "Category=Integration" --configuration Release --verbosity normal --logger "console;verbosity=detailed"

$integrationTestExitCode = $LASTEXITCODE

# Run all tests together
Write-Host "Running All Tests..." -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow
dotnet test Email.Tests/Email.Tests.csproj --configuration Release --verbosity normal --logger "console;verbosity=detailed"

$allTestsExitCode = $LASTEXITCODE

# Summary
Write-Host "`nTest Summary:" -ForegroundColor Green
Write-Host "=============" -ForegroundColor Green
Write-Host "Unit Tests Exit Code: $unitTestExitCode" -ForegroundColor $(if ($unitTestExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "Integration Tests Exit Code: $integrationTestExitCode" -ForegroundColor $(if ($integrationTestExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "All Tests Exit Code: $allTestsExitCode" -ForegroundColor $(if ($allTestsExitCode -eq 0) { "Green" } else { "Red" })

if ($allTestsExitCode -eq 0) {
    Write-Host "`nAll tests passed successfully! 🎉" -ForegroundColor Green
} else {
    Write-Host "`nSome tests failed! ❌" -ForegroundColor Red
    exit 1
}
