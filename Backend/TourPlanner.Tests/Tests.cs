using NUnit.Framework;
using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Application.Contracts.Routing;
using TourPlanner.Application.Contracts.Security;
using TourPlanner.Application.Contracts.Time;
using TourPlanner.Application.UseCases.Auth.Login;
using TourPlanner.Application.UseCases.Auth.Register;
using TourPlanner.Application.UseCases.TourLogs.CreateTourLog;
using TourPlanner.Application.UseCases.TourLogs.UpdateTourLog;
using TourPlanner.Application.UseCases.Tours.CreateTour;
using TourPlanner.Application.UseCases.Tours.UpdateTour;
using TourPlanner.Domain;
using TourPlanner.Domain.Entities;
using TourPlanner.Domain.Metrics;
using DomainTransportType = TourPlanner.Domain.Enums.TransportType;
using ContractRoutePlan = TourPlanner.Application.Contracts.Routing.RoutePlan;

namespace TourPlanner.Tests;

public sealed class Tests
{
    [TestFixture]
    public sealed class AuthUseCaseTests
    {
        [Test]
        public async Task RegisterAndLogin_CreateTokensAndPersistSessions()
        {
            var clock = new FakeClock();
            var users = new InMemoryUserRepository();
            var sessions = new InMemoryUserSessionRepository();
            var useCase = new RegisterUseCase(new RegisterRequestValidator(), users, new FakePasswordHasher(), new FakeTokenService(clock), sessions, new FakeUnitOfWork());

            var registered = await useCase.ExecuteAsync(new RegisterRequestDto("alice", "secret123"));
            Assert.That(registered.UserName, Is.EqualTo("alice"));
            Assert.That(users.Users, Has.Count.EqualTo(1));
            Assert.That(sessions.Sessions, Has.Count.EqualTo(1));

            var loginUseCase = new LoginUseCase(users, new FakePasswordHasher(), new FakeTokenService(clock), sessions, new FakeUnitOfWork());
            var loggedIn = await loginUseCase.ExecuteAsync(new LoginRequestDto("alice", "secret123"));
            Assert.That(loggedIn.UserName, Is.EqualTo("alice"));
            Assert.That(sessions.Sessions, Has.Count.EqualTo(2));
        }
    }

    [TestFixture]
    public sealed class TourUseCaseTests
    {
        [Test]
        public async Task CreateTour_UsesRouteServiceAndReturnsSummary()
        {
            var user = User.Create("alice", "hash");
            _ = new InMemoryUserRepository(user);
            var tours = new InMemoryTourRepository();
            var currentUser = new FakeCurrentUserContext(user.Id, user.UserName);
            var useCase = new CreateTourUseCase(tours, new FixedRouteService(), currentUser, new FakeUnitOfWork());

            var result = await useCase.ExecuteAsync(new CreateTourRequest("City Loop", "A short city tour", "Vienna", "Graz", DomainTransportType.Bicycling));

            Assert.That(result.Name, Is.EqualTo("City Loop"));
            Assert.That(result.DistanceKm, Is.EqualTo(12.4).Within(0.01));
            Assert.That(tours.Items, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task UpdateTour_UsesRouteServiceAndReturnsUpdatedSummary()
        {
            var user = User.Create("alice", "hash");
            var tour = CreateTour(user.Id, "Original", 1, 90);
            var tours = new InMemoryTourRepository(tour);
            var currentUser = new FakeCurrentUserContext(user.Id, user.UserName);
            var useCase = new UpdateTourUseCase(tours, new FixedRouteService(), currentUser, new FakeUnitOfWork());

            var updated = await useCase.ExecuteAsync(new UpdateTourRequest(tour.Id, "Updated", "Updated desc", "Vienna", "Linz", DomainTransportType.Walking));

            Assert.That(updated.Name, Is.EqualTo("Updated"));
            Assert.That(updated.From, Is.EqualTo("Vienna"));
            Assert.That(updated.To, Is.EqualTo("Linz"));
            Assert.That(tour.DistanceKm, Is.EqualTo(12.4).Within(0.01));
        }

        private static Tour CreateTour(Guid userId, string name, int popularity, double childFriendliness)
        {
            var tour = Tour.Create(userId,
                name,
                "desc",
                "A",
                "B",
                DomainTransportType.Walking,
                1,
                1,
                "route",
                null,
                null,
                null);
            tour.UpdateMetrics(popularity, childFriendliness);
            return tour;
        }
    }

    [TestFixture]
    public sealed class TourLogUseCaseTests
    {
        [Test]
        public async Task CreateLog_RecalculatesTourMetrics()
        {
            var user = User.Create("alice", "hash");
            var tour = Tour.Create(
                user.Id,
                "Tour",
                "desc",
                "A",
                "B",
                DomainTransportType.Walking,
                5,
                60,
                "route",
                null,
                null,
                null);
            var tours = new InMemoryTourRepository(tour);
            var logs = new InMemoryTourLogRepository();
            var currentUser = new FakeCurrentUserContext(user.Id, user.UserName);
            var useCase = new CreateTourLogUseCase(tours, logs, currentUser, new FakeUnitOfWork());

            await useCase.ExecuteAsync(new CreateTourLogRequest(tour.Id, DateTimeOffset.UtcNow, "Nice trip", TourDifficulty.Easy, 5, 60, 5));

            Assert.That(tour.Popularity, Is.EqualTo(1));
            Assert.That(tour.ChildFriendliness, Is.LessThanOrEqualTo(100).And.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task UpdateLog_RecalculatesTourMetrics()
        {
            var user = User.Create("alice", "hash");
            var tour = Tour.Create(
                user.Id,
                "Tour",
                "desc",
                "A",
                "B",
                DomainTransportType.Walking,
                5,
                60,
                "route",
                null,
                null,
                null);
            var tours = new InMemoryTourRepository(tour);
            var logs = new InMemoryTourLogRepository();
            var currentUser = new FakeCurrentUserContext(user.Id, user.UserName);
            var createUseCase = new CreateTourLogUseCase(tours, logs, currentUser, new FakeUnitOfWork());
            var created = await createUseCase.ExecuteAsync(new CreateTourLogRequest(tour.Id, DateTimeOffset.UtcNow, "Nice trip", TourDifficulty.Easy, 5, 60, 5));

            var updateUseCase = new UpdateTourLogUseCase(tours, logs, currentUser, new FakeUnitOfWork());
            var updated = await updateUseCase.ExecuteAsync(new UpdateTourLogRequest(logs.Items[0].Id, DateTimeOffset.UtcNow.AddDays(1), "Better trip", TourDifficulty.Medium, 7, 90, 4));

            Assert.That(created.Comment, Is.EqualTo("Nice trip"));
            Assert.That(updated.Comment, Is.EqualTo("Better trip"));
            Assert.That(tour.Popularity, Is.EqualTo(1));
        }
    }

    [TestFixture]
    public sealed class TourMetricsCalculatorTests
    {
        [Test]
        public void Calculate_EmptyLogs_ReturnsNeutralScore()
        {
            var metrics = TourMetricsCalculator.Calculate(Array.Empty<TourLog>());

            Assert.That(metrics.LogCount, Is.EqualTo(0));
            Assert.That(metrics.ChildFriendliness, Is.EqualTo(100));
        }
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-05-28T12:00:00Z");
    }

    private sealed class FakeCurrentUserContext(Guid userId, string userName) : ICurrentUserContext
    {
        public bool IsAuthenticated => true;

        public Guid UserId { get; } = userId;

        public string UserName { get; } = userName;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string hashedPassword, string providedPassword) => Hash(providedPassword) == hashedPassword;
    }

    private sealed class FakeTokenService(FakeClock clock) : ITokenService
    {
        public Task<TokenPair> GenerateTokenPairAsync(User user, CancellationToken cancellationToken = default)
            => Task.FromResult(new TokenPair($"access:{user.UserName}", clock.UtcNow.AddMinutes(15), $"refresh:{user.UserName}:{Guid.NewGuid():N}", clock.UtcNow.AddHours(1)));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class InMemoryUserRepository(params User[] initialUsers) : IUserRepository
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

    private sealed class InMemoryTourRepository(params Tour[] initialTours) : ITourRepository
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

    private sealed class NoOpFileStorage : IFileStorage
    {
        public Task<string> SaveFileAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
            => Task.FromResult(fileName);

        public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<byte[]?> ReadFileAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult<byte[]?>(null);
    }

    private sealed class FixedRouteService : IOpenRouteService
    {
        public Task<ContractRoutePlan> BuildRouteAsync(string from, string to, DomainTransportType transportType, CancellationToken cancellationToken = default)
            => Task.FromResult(new ContractRoutePlan(
                12.4,
                38.0,
                $"{{\"from\":\"{from}\",\"to\":\"{to}\"}}",
                "Route Summary Text",
                48.2082, 16.3738,
                47.0707, 15.4395
                ));
    }
}










