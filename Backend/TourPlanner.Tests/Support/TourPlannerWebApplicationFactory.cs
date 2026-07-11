using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Tests.Support;

public sealed class TourPlannerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _storagePath;
    private readonly Dictionary<string, string?> _previousEnvironmentVariables = [];
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryUserSessionRepository _sessions = new();
    private readonly InMemoryTourRepository _tours = new();
    private readonly InMemoryTourLogRepository _tourLogs = new();

    public TourPlannerWebApplicationFactory(string databaseName)
    {
        _storagePath = Path.Combine(Path.GetTempPath(), $"tourplanner-tests-{databaseName}");

        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Database=tourplanner-tests;Username=test;Password=test");
        SetEnvironmentVariable("Jwt__Issuer", "TourPlanner");
        SetEnvironmentVariable("Jwt__Audience", "TourPlanner.Client");
        SetEnvironmentVariable("Jwt__SigningKey", "integration-test-signing-key-which-is-long-enough");
        SetEnvironmentVariable("Jwt__AccessTokenMinutes", "60");
        SetEnvironmentVariable("Jwt__RefreshTokenDays", "14");
        SetEnvironmentVariable("Storage__BasePath", _storagePath);
        SetEnvironmentVariable("OpenRouteService__BaseUrl", "https://api.openrouteservice.org");
        SetEnvironmentVariable("OpenRouteService__ApiKey", string.Empty);
        SetEnvironmentVariable("DisableDatabaseMigrations", "true");
    }

    private void SetEnvironmentVariable(string name, string? value)
    {
        _previousEnvironmentVariables[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TourPlannerDbContext>>();
            services.RemoveAll<TourPlannerDbContext>();
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IUserSessionRepository>();
            services.RemoveAll<ITourRepository>();
            services.RemoveAll<ITourLogRepository>();
            services.RemoveAll<IUnitOfWork>();

            services.AddSingleton<IUserRepository>(_users);
            services.AddSingleton<IUserSessionRepository>(_sessions);
            services.AddSingleton<ITourRepository>(_tours);
            services.AddSingleton<ITourLogRepository>(_tourLogs);
            services.AddSingleton<IUnitOfWork>(new NoOpUnitOfWork());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        foreach (var (name, value) in _previousEnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        if (Directory.Exists(_storagePath))
        {
            Directory.Delete(_storagePath, recursive: true);
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.FirstOrDefault(user => user.Id == id));

        public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.FirstOrDefault(user => user.UserName == userName));

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryUserSessionRepository : IUserSessionRepository
    {
        public List<UserSession> Sessions { get; } = [];

        public Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult(Sessions.FirstOrDefault(session => session.RefreshToken == refreshToken));

        public Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
        {
            Sessions.Add(session);
            return Task.CompletedTask;
        }

        public void Remove(UserSession session) => Sessions.Remove(session);
    }

    private sealed class InMemoryTourRepository : ITourRepository
    {
        public List<Tour> Items { get; } = [];

        public Task<Tour?> GetByIdAsync(Guid tourId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(tour => tour.Id == tourId && tour.UserId == userId));

        public Task<IReadOnlyList<Tour>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<Tour>)Items.Where(tour => tour.UserId == userId).ToList());

        public Task AddAsync(Tour tour, CancellationToken cancellationToken = default)
        {
            Items.Add(tour);
            return Task.CompletedTask;
        }

        public void Remove(Tour tour) => Items.Remove(tour);
    }

    private sealed class InMemoryTourLogRepository : ITourLogRepository
    {
        public List<TourLog> Items { get; } = [];

        public Task<TourLog?> GetByIdAsync(Guid tourLogId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(log => log.Id == tourLogId));

        public Task<IReadOnlyList<TourLog>> GetByTourIdAsync(Guid tourId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<TourLog>)Items.Where(log => log.TourId == tourId).ToList());

        public Task<IReadOnlyList<TourLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<TourLog>)Items.ToList());

        public Task AddAsync(TourLog tourLog, CancellationToken cancellationToken = default)
        {
            Items.Add(tourLog);
            return Task.CompletedTask;
        }

        public void Remove(TourLog tourLog) => Items.Remove(tourLog);
    }
}






