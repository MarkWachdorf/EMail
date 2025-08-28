#!/usr/bin/env pwsh

Write-Host "🚀 Starting Email Microservice API..." -ForegroundColor Green
Write-Host "📖 Swagger UI will open automatically in your browser" -ForegroundColor Cyan
Write-Host "🌐 API URL: https://localhost:7198" -ForegroundColor Yellow
Write-Host "📚 Swagger URL: https://localhost:7198/swagger" -ForegroundColor Yellow
Write-Host ""

# Build the solution first
Write-Host "🔨 Building solution..." -ForegroundColor Blue
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Run the API
Write-Host "🚀 Starting API..." -ForegroundColor Blue
dotnet run --project Email.API/Email.API.csproj --launch-profile https
