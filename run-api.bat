@echo off
echo 🚀 Starting Email Microservice API...
echo 📖 Swagger UI will open automatically in your browser
echo 🌐 API URL: https://localhost:7198
echo 📚 Swagger URL: https://localhost:7198/swagger
echo.

echo 🔨 Building solution...
dotnet build

if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed!
    pause
    exit /b 1
)

echo ✅ Build successful!
echo.

echo 🚀 Starting API...
dotnet run --project Email.API/Email.API.csproj --launch-profile https
