using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Application.Contracts.Routing;
using TourPlanner.Application.Contracts.Security;
using TourPlanner.Application.Contracts.Time;
using TourPlanner.Infrastructure.Options;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Services;

namespace TourPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTourPlannerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["CONNECTION_STRING"]
            ?? throw new InvalidOperationException("The default database connection string is not configured.");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.Configure<OpenRouteOptions>(configuration.GetSection("OpenRouteService"));

        services.AddHttpContextAccessor();
        services.AddDbContext<TourPlannerDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TourPlannerDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<ITourLogRepository, TourLogRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IFileStorage, FileStorage>();
        services.AddHttpClient<IOpenRouteService, OpenRouteServiceClient>((sp, client) =>
        {
            var openRouteOptions = sp.GetRequiredService<IOptions<OpenRouteOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(openRouteOptions.BaseUrl))
            {
                client.BaseAddress = new Uri(openRouteOptions.BaseUrl);
            }
        });

        services.AddHostedService<DatabaseCleanupService>();

        return services;
    }
}

