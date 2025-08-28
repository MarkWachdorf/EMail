using Email.API;
using Email.Application.DependencyInjection;
using Email.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Email.Tests.Integration;

public class EmailIntegrationTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EmailTestDb;Trusted_Connection=true;MultipleActiveResultSets=true",
                ["Infrastructure:EnableRetryPolicy"] = "false",
                ["Infrastructure:EnableConnectionPooling"] = "false",
                ["Infrastructure:ConnectionTimeout"] = "5",
                ["Infrastructure:CommandTimeout"] = "5"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing infrastructure services
            var infrastructureDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService));
            if (infrastructureDescriptor != null)
            {
                services.Remove(infrastructureDescriptor);
            }

            // Add test services
            services.AddInfrastructureServices(GetTestConfiguration(), options =>
            {
                options.ConnectionStringName = "DefaultConnection";
                options.EnableRetryPolicy = false;
                options.MaxRetryAttempts = 1;
                options.RetryDelaySeconds = 1;
                options.EnableConnectionPooling = false;
                options.MaxPoolSize = 1;
                options.MinPoolSize = 0;
                options.ConnectionTimeout = 5;
                options.CommandTimeout = 5;
            });

            services.AddApplicationServices();
        });

        builder.UseEnvironment("Test");
    }

    private IConfiguration GetTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=EmailTestDb;Trusted_Connection=true;MultipleActiveResultSets=true",
                ["Infrastructure:EnableRetryPolicy"] = "false",
                ["Infrastructure:EnableConnectionPooling"] = "false",
                ["Infrastructure:ConnectionTimeout"] = "5",
                ["Infrastructure:CommandTimeout"] = "5"
            })
            .Build();
    }
}
