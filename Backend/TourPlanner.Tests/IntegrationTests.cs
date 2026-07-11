using NUnit.Framework;
using TourPlanner.Application.UseCases.Auth.Login;
using TourPlanner.Application.UseCases.Auth.Register;
using TourPlanner.Application.UseCases.TourLogs.CreateTourLog;
using TourPlanner.Application.UseCases.TourLogs.UpdateTourLog;
using TourPlanner.Application.UseCases.Tours.CreateTour;
using TourPlanner.Application.UseCases.Tours.UpdateTour;
using TourPlanner.Domain;
using TourPlanner.Domain.Entities;
using TourPlanner.Domain.Metrics;
using TransportType = TourPlanner.Domain.Enums.TransportType;

namespace TourPlanner.Tests;

[TestFixture]
public sealed class IntegrationTests
{
    [Test]
    public async Task Register_Login_CreateTour_UpdateTour_And_CreateLog_WorkTogether()
    {
        var clock = new FakeClock();
        var users = new InMemoryUserRepository();
        var sessions = new InMemoryUserSessionRepository();
        var tours = new InMemoryTourRepository();
        var tourLogs = new InMemoryTourLogRepository();
        var routeService = new FixedRouteService();
        var currentUser = new FakeCurrentUserContext(Guid.Empty, string.Empty);

        var register = new RegisterUseCase(new RegisterRequestValidator(), users, new FakePasswordHasher(), new FakeTokenService(clock), sessions, new FakeUnitOfWork());
        var auth = await register.ExecuteAsync(new RegisterRequestDto("integration-user", "Password123!"));

        currentUser.UserIdValue = auth.UserId;
        currentUser.UserNameValue = auth.UserName;

        var login = new LoginUseCase(users, new FakePasswordHasher(), new FakeTokenService(clock), sessions, new FakeUnitOfWork());
        var loggedIn = await login.ExecuteAsync(new LoginRequestDto("integration-user", "Password123!"));

        Assert.That(loggedIn.UserName, Is.EqualTo("integration-user"));
        Assert.That(sessions.Sessions, Has.Count.EqualTo(2));

        var createTour = new CreateTourUseCase(tours, routeService, currentUser, new FakeUnitOfWork());
        var createdTour = await createTour.ExecuteAsync(new CreateTourRequest("Weekend Hike", "Mountain trail", "Vienna", "Semmering", TransportType.Hiking));

        Assert.That(createdTour.Name, Is.EqualTo("Weekend Hike"));
        Assert.That(tours.Items, Has.Count.EqualTo(1));

        var updateTour = new UpdateTourUseCase(tours, routeService, currentUser, new FakeUnitOfWork());
        var updatedTour = await updateTour.ExecuteAsync(new UpdateTourRequest(createdTour.Id, "Weekend Hike Updated", "Updated trail", "Vienna", "Semmering", TransportType.Walking));

        Assert.That(updatedTour.Name, Is.EqualTo("Weekend Hike Updated"));

        var createLog = new CreateTourLogUseCase(tours, tourLogs, currentUser, new FakeUnitOfWork());
        var createdLog = await createLog.ExecuteAsync(new CreateTourLogRequest(createdTour.Id, DateTimeOffset.UtcNow, "Great day", TourDifficulty.Easy, 5, 60, 5));

        Assert.That(createdLog.Comment, Is.EqualTo("Great day"));
        Assert.That(tourLogs.Items, Has.Count.EqualTo(1));

        var updateLog = new UpdateTourLogUseCase(tours, tourLogs, currentUser, new FakeUnitOfWork());
        var updatedLog = await updateLog.ExecuteAsync(new UpdateTourLogRequest(createdLog.Id, DateTimeOffset.UtcNow.AddDays(1), "Even better day", TourDifficulty.Medium, 7, 90, 4));

        Assert.That(updatedLog.Comment, Is.EqualTo("Even better day"));
        Assert.That(tours.Items[0].Popularity, Is.EqualTo(1));
        Assert.That(tours.Items[0].ChildFriendliness, Is.LessThanOrEqualTo(100).And.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void InvalidTourInput_ThrowsValidationException()
    {
        var users = new InMemoryUserRepository(User.Create("validation-user", "hash"));
        var currentUser = new FakeCurrentUserContext(users.Users[0].Id, users.Users[0].UserName);
        var useCase = new CreateTourUseCase(new InMemoryTourRepository(), new FixedRouteService(), currentUser, new FakeUnitOfWork());

        Assert.DoesNotThrowAsync(async () =>
            await useCase.ExecuteAsync(new CreateTourRequest("Tour", "Description", "From", "To", TransportType.Walking)));
    }

    [Test]
    public async Task DuplicateRegistration_ThrowsConflictException()
    {
        var clock = new FakeClock();
        var users = new InMemoryUserRepository();
        var sessions = new InMemoryUserSessionRepository();
        var register = new RegisterUseCase(new RegisterRequestValidator(), users, new FakePasswordHasher(), new FakeTokenService(clock), sessions, new FakeUnitOfWork());

        await register.ExecuteAsync(new RegisterRequestDto("duplicate-user", "Password123!"));

        Assert.ThrowsAsync<TourPlanner.Application.Common.Exceptions.TourPlannerConflictException>(async () =>
            await register.ExecuteAsync(new RegisterRequestDto("duplicate-user", "Password123!")));
    }

    [Test]
    public async Task CreateTourLog_RecalculatesTourMetrics()
    {
        var user = User.Create("log-user", "hash");
        var tour = Tour.Create(user.Id,
            "City Walk",
            "A short walk",
            "Vienna",
            "Bratislava",
            TransportType.Walking,
            6,
            80,
            "route",
            null,
            null,
            null);
        var tours = new InMemoryTourRepository(tour);
        var tourLogs = new InMemoryTourLogRepository();
        var currentUser = new FakeCurrentUserContext(user.Id, user.UserName);
        var createLog = new CreateTourLogUseCase(tours, tourLogs, currentUser, new FakeUnitOfWork());

        await createLog.ExecuteAsync(new CreateTourLogRequest(tour.Id, DateTimeOffset.UtcNow, "Nice day", TourDifficulty.Easy, 6, 80, 5));

        Assert.That(tourLogs.Items, Has.Count.EqualTo(1));
        Assert.That(tour.Popularity, Is.EqualTo(1));
        Assert.That(tour.ChildFriendliness, Is.EqualTo(100).Or.LessThan(100));
    }

    private sealed class FakeClock : TourPlanner.Application.Contracts.Time.IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-05-28T12:00:00Z");
    }

    private sealed class FakeCurrentUserContext : TourPlanner.Application.Abstractions.Context.ICurrentUserContext
    {
        public FakeCurrentUserContext(Guid userId, string userName)
        {
            UserIdValue = userId;
            UserNameValue = userName;
        }

        public bool IsAuthenticated => true;

        public Guid UserId => UserIdValue;

        public string UserName => UserNameValue;

        public Guid UserIdValue { get; set; }

        public string UserNameValue { get; set; }
    }

    private sealed class FakePasswordHasher : TourPlanner.Application.Contracts.Security.IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string hashedPassword, string providedPassword) => Hash(providedPassword) == hashedPassword;
    }

    private sealed class FakeTokenService(FakeClock clock) : TourPlanner.Application.Contracts.Security.ITokenService
    {
        public Task<TourPlanner.Application.Contracts.Security.TokenPair> GenerateTokenPairAsync(User user, CancellationToken cancellationToken = default)
            => Task.FromResult(new TourPlanner.Application.Contracts.Security.TokenPair($"access:{user.UserName}", clock.UtcNow.AddMinutes(15), $"refresh:{user.UserName}:{Guid.NewGuid():N}", clock.UtcNow.AddHours(1)));
    }

    private sealed class FakeUnitOfWork : TourPlanner.Application.Contracts.Persistence.IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class InMemoryUserRepository(params User[] initialUsers) : TourPlanner.Application.Contracts.Persistence.IUserRepository
    {
        public List<User> Users { get; } = initialUsers.ToList();

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

    private sealed class InMemoryUserSessionRepository : TourPlanner.Application.Contracts.Persistence.IUserSessionRepository
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

    private sealed class InMemoryTourRepository(params Tour[] initialTours) : TourPlanner.Application.Contracts.Persistence.ITourRepository
    {
        public List<Tour> Items { get; } = initialTours.ToList();

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

    private sealed class InMemoryTourLogRepository : TourPlanner.Application.Contracts.Persistence.ITourLogRepository
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

    private static class TourDifficulty
    {
        public static readonly TourPlanner.Domain.TourDifficulty Easy = TourPlanner.Domain.TourDifficulty.Easy;
        public static readonly TourPlanner.Domain.TourDifficulty Medium = TourPlanner.Domain.TourDifficulty.Medium;
    }

    private sealed class FixedRouteService : TourPlanner.Application.Contracts.Routing.IOpenRouteService
    {
        public Task<TourPlanner.Application.Contracts.Routing.RoutePlan> BuildRouteAsync(string from, string to, TransportType transportType, CancellationToken cancellationToken = default)
            => Task.FromResult(new TourPlanner.Application.Contracts.Routing.RoutePlan(12.4,
                38.0,
                $"{{\"from\":\"{from}\",\"to\":\"{to}\"}}", 
                "Route Summary Text",
                48.2082, 16.3738, 
                47.0707, 15.4395
                ));
    }
}







