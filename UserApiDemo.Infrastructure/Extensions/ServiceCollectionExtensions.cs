using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using UserApiDemo.Application.Interfaces;
using UserApiDemo.Application.Services;
using UserApiDemo.Domain.Interfaces;
using UserApiDemo.Persistence.Data;
using UserApiDemo.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace UserApiDemo.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                    // Add retries to smooth over transient startup issues when SQL container is warming up
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        });

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        // Auto migrate database
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Infrastructure.Migrations");

            try
            {
                var migrations = dbContext.Database.GetMigrations().ToList();
                if (!migrations.Any())
                {
                    logger.LogWarning("No EF migrations found. Running EnsureCreated() to create schema from the current model.");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database created via EnsureCreated.");
                }
                else
                {
                    logger.LogInformation("Applying database migrations (with retry)... Migrations found: {Migrations}", string.Join(", ", migrations));
                    dbContext.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply database migrations.");
                throw;
            }
        }

        return app;
    }
}
