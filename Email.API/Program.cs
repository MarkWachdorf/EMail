using Email.Infrastructure.DependencyInjection;
using Email.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Email Microservice API",
        Version = "v1",
        Description = "A comprehensive email microservice for sending, caching, and managing email messages.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Email Service Team",
            Email = "support@example.com"
        }
    });
    
    // Include XML comments from the API project
    var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
    {
        c.IncludeXmlComments(apiXmlPath);
    }
    
    // Include XML comments from the Contracts project
    var contractsAssembly = typeof(Email.Contracts.Requests.CreateEmailRequest).Assembly;
    var contractsXmlFile = $"{contractsAssembly.GetName().Name}.xml";
    var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, contractsXmlFile);
    if (File.Exists(contractsXmlPath))
    {
        c.IncludeXmlComments(contractsXmlPath);
    }
});

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration, options =>
{
    options.ConnectionStringName = "DefaultConnection";
    options.EnableRetryPolicy = true;
    options.MaxRetryAttempts = 3;
    options.RetryDelaySeconds = 1;
    options.EnableConnectionPooling = true;
    options.MaxPoolSize = 100;
    options.MinPoolSize = 0;
    options.ConnectionTimeout = 30;
    options.CommandTimeout = 30;
});

// Add application services (business logic layer)
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Microservice API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Email Microservice API Documentation";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
