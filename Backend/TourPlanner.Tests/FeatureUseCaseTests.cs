using System.Text;
using System.Text.Json;
using NSubstitute;
using NUnit.Framework;
using TourPlanner.Application.Abstractions.Context;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.CommonDtos.TourLogs;
using TourPlanner.Application.CommonDtos.Tours;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Application.Contracts.Security;
using TourPlanner.Application.Contracts.Time;
using TourPlanner.Application.UseCases.Auth.Refresh;
using TourPlanner.Application.UseCases.Search.TourSearch;
using TourPlanner.Application.UseCases.TourLogs.DeleteTourLog;
using TourPlanner.Application.UseCases.TourLogs.GetTourLogById;
using TourPlanner.Application.UseCases.TourLogs.GetTourLogsByTour;
using TourPlanner.Application.UseCases.Tours.DeleteTour;
using TourPlanner.Application.UseCases.Tours.ExportTours;
using TourPlanner.Application.UseCases.Tours.GetAllTours;
using TourPlanner.Application.UseCases.Tours.GetRecommendedTours;
using TourPlanner.Application.UseCases.Tours.GetTourById;
using TourPlanner.Application.UseCases.Tours.GetTourInsights;
using TourPlanner.Application.UseCases.Tours.ImportTours;
using TourPlanner.Application.UseCases.Tours.UploadTourImage;
using TourPlanner.Domain;
using TourPlanner.Domain.Entities;
using TourPlanner.Domain.Metrics;
using DomainTransportType = TourPlanner.Domain.Enums.TransportType;

namespace TourPlanner.Tests;

public sealed class FeatureUseCaseTests
{
    [TestFixture]
    public sealed class AuthRefreshTests
    {
        [Test]
        public async Task RefreshUseCase_RotatesTokensAndPersistsSession()
        {
            var clock = Substitute.For<IClock>();
            clock.UtcNow.Returns(DateTimeOffset.Parse("2026-07-02T10:00:00Z"));

            var user = User.Create("alice", "hash");
            var session = UserSession.Create(user.Id, "old-refresh", clock.UtcNow.AddMinutes(10));

            var users = Substitute.For<IUserRepository>();
            users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<User?>(user));

            var sessions = Substitute.For<IUserSessionRepository>();
            sessions.GetByRefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>()).Returns(Task.FromResult<UserSession?>(session));

            var tokenService = Substitute.For<ITokenService>();
            var refreshedExpiresAt = clock.UtcNow.AddHours(1);
            var accessTokenExpiresAt = clock.UtcNow.AddMinutes(15);
            tokenService.GenerateTokenPairAsync(user, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new TokenPair("new-access", accessTokenExpiresAt, "new-refresh", refreshedExpiresAt)));

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new RefreshUseCase(sessions, clock, users, tokenService, unitOfWork);

            var result = await useCase.ExecuteAsync(new RefreshTokenRequestDto("old-refresh"));

            Assert.That(result.UserId, Is.EqualTo(user.Id));
            Assert.That(result.AccessToken, Is.EqualTo("new-access"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-refresh"));
            Assert.That(session.RefreshToken, Is.EqualTo("new-refresh"));
            Assert.That(session.LastSeenAt, Is.Not.Null);
            await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void RefreshUseCase_RejectsExpiredSession()
        {
            var clock = Substitute.For<IClock>();
            clock.UtcNow.Returns(DateTimeOffset.Parse("2026-07-02T10:00:00Z"));

            var user = User.Create("alice", "hash");
            var session = UserSession.Create(user.Id, "old-refresh", clock.UtcNow.AddMinutes(-1));

            var sessions = Substitute.For<IUserSessionRepository>();
            sessions.GetByRefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>()).Returns(Task.FromResult<UserSession?>(session));

            var useCase = new RefreshUseCase(sessions, clock, Substitute.For<IUserRepository>(), Substitute.For<ITokenService>(), Substitute.For<IUnitOfWork>());

            Assert.ThrowsAsync<TourPlannerUnauthorizedException>(() => useCase.ExecuteAsync(new RefreshTokenRequestDto("old-refresh")));
        }
    }

    [TestFixture]
    public sealed class TourReadAndMutationTests
    {
        [Test]
        public async Task GetAllToursUseCase_ReturnsCurrentUsersSummaries()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tourA = CreateTour(user.Id, "Bike tour", 2, 55, imagePath: "images/a.jpg");
            var tourB = CreateTour(user.Id, "Hike tour", 5, 80);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<Tour>)new[] { tourA, tourB }.ToList()));

            var useCase = new GetAllToursUseCase(tours, currentUser);
            var result = await useCase.ExecuteAsync(new GetAllToursRequest());

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Bike tour"));
            Assert.That(result[1].ImagePath, Is.Null);
        }

        [Test]
        public async Task GetRecommendedToursUseCase_SortsByChildFriendlinessThenPopularityAndAppliesLimit()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var bestChild = CreateTour(user.Id, "Most child friendly", popularity: 3, childFriendliness: 95);
            var tieBreaker = CreateTour(user.Id, "Best popularity", popularity: 9, childFriendliness: 95);
            var lower = CreateTour(user.Id, "Lower score", popularity: 10, childFriendliness: 80);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<Tour>)new[] { lower, bestChild, tieBreaker }.ToList()));

            var useCase = new GetRecommendedToursUseCase(tours, currentUser);
            var result = await useCase.ExecuteAsync(new GetRecommendedToursRequest(2));

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Best popularity"));
            Assert.That(result[1].Name, Is.EqualTo("Most child friendly"));
        }

        [Test]
        public async Task GetTourByIdUseCase_ReturnsTourWithLogs()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Weekend ride", popularity: 7, childFriendliness: 72, imagePath: "tour-images/ride.jpg");
            var logs = new List<TourLog>
            {
                CreateLog(tour.Id, "Nice weather", TourDifficulty.Easy, 11, 55, 5)
            };

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByTourIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<TourLog>)logs));

            var useCase = new GetTourByIdUseCase(tours, tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new GetTourByIdRequest(tour.Id));

            Assert.That(result.Id, Is.EqualTo(tour.Id));
            Assert.That(result.Logs, Has.Count.EqualTo(1));
            Assert.That(result.Logs[0].Comment, Is.EqualTo("Nice weather"));
            Assert.That(result.ImagePath, Is.EqualTo("tour-images/ride.jpg"));
        }

        [Test]
        public async Task DeleteTourUseCase_DeletesImageAndRemovesTour()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Tour to delete", popularity: 1, childFriendliness: 50, imagePath: "tour-images/delete-me.jpg");

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var fileStorage = Substitute.For<IFileStorage>();
            fileStorage.DeleteFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new DeleteTourUseCase(tours, fileStorage, currentUser, unitOfWork);
            await useCase.ExecuteAsync(new DeleteTourRequest(tour.Id));

            await fileStorage.Received(1).DeleteFileAsync("tour-images/delete-me.jpg", Arg.Any<CancellationToken>());
            tours.Received(1).Remove(tour);
            await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task UploadTourImageUseCase_SanitizesFilenameAndUpdatesTour()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Tour with image", popularity: 1, childFriendliness: 60);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var fileStorage = Substitute.For<IFileStorage>();
            var expectedPath = Path.Combine("tour-images", user.Id.ToString("N"), tour.Id.ToString("N"), "evil.jpg").Replace('\\', '/');
            fileStorage.SaveFileAsync(expectedPath, Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedPath));

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new UploadTourImageUseCase(tours, fileStorage, currentUser, unitOfWork);
            var result = await useCase.ExecuteAsync(new UploadTourImageRequest(tour.Id, "../../evil.jpg", new byte[] { 1, 2, 3 }));

            Assert.That(result.ImagePath, Is.EqualTo(expectedPath));
            Assert.That(tour.ImagePath, Is.EqualTo(expectedPath));
            await fileStorage.Received(1).SaveFileAsync(expectedPath, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
            await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetTourInsightsUseCase_UpdatesTourMetrics()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Insight tour", popularity: 0, childFriendliness: 100);
            var logs = new[]
            {
                CreateLog(tour.Id, "First log", TourDifficulty.Easy, 10, 45, 4),
                CreateLog(tour.Id, "Second log", TourDifficulty.Medium, 12, 60, 5)
            };

            var expected = TourMetricsCalculator.Calculate(logs);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByTourIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<TourLog>)logs.ToList()));

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new GetTourInsightsUseCase(tours, tourLogs, currentUser, unitOfWork);
            var result = await useCase.ExecuteAsync(new GetTourInsightsRequest(tour.Id));

            Assert.That(result.LogCount, Is.EqualTo(expected.LogCount));
            Assert.That(result.AverageDifficulty, Is.EqualTo(expected.AverageDifficulty).Within(0.0001));
            Assert.That(result.ChildFriendliness, Is.EqualTo(expected.ChildFriendliness).Within(0.0001));
            Assert.That(tour.Popularity, Is.EqualTo(expected.Popularity));
            Assert.That(tour.ChildFriendliness, Is.EqualTo(expected.ChildFriendliness).Within(0.0001));
        }
    }

    [TestFixture]
    public sealed class TourLogAndSearchTests
    {
        [Test]
        public async Task GetTourLogByIdUseCase_ReturnsMappedLog()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Tour", popularity: 1, childFriendliness: 80);
            var log = CreateLog(tour.Id, "Great climb", TourDifficulty.Hard, 14, 90, 5);

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByIdAsync(log.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<TourLog?>(log));

            var useCase = new GetTourLogByIdUseCase(tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new GetTourLogByIdRequest(log.Id));

            Assert.That(result.Id, Is.EqualTo(log.Id));
            Assert.That(result.Comment, Is.EqualTo("Great climb"));
            Assert.That(result.Rating, Is.EqualTo(5));
        }

        [Test]
        public async Task GetTourLogsByTourIdUseCase_ReturnsLogsForTour()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Log tour", popularity: 1, childFriendliness: 60);
            var logA = CreateLog(tour.Id, "Climb", TourDifficulty.Medium, 10, 60, 4);
            var logB = CreateLog(tour.Id, "Return", TourDifficulty.Easy, 8, 45, 5);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByTourIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<TourLog>)new[] { logA, logB }.ToList()));

            var useCase = new GetTourLogsByTourIdUseCase(tours, tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new GetTourLogsByTourIdRequest(tour.Id));

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Comment, Is.EqualTo("Climb"));
            Assert.That(result[1].Comment, Is.EqualTo("Return"));
        }

        [Test]
        public async Task DeleteTourLogUseCase_RemovesLogAndRecalculatesMetrics()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Tour", popularity: 0, childFriendliness: 100);
            var remainingLog = CreateLog(tour.Id, "Keep", TourDifficulty.Easy, 10, 45, 5);
            var deletedLog = CreateLog(tour.Id, "Remove", TourDifficulty.Hard, 20, 90, 2);
            var logStore = new List<TourLog> { remainingLog, deletedLog };

            var tours = Substitute.For<ITourRepository>();
            tours.GetByIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tour?>(tour));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByIdAsync(deletedLog.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<TourLog?>(deletedLog));
            tourLogs.GetByTourIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult((IReadOnlyList<TourLog>)logStore.Where(log => log.TourId == tour.Id).ToList()));
            tourLogs.When(x => x.Remove(Arg.Any<TourLog>()))
                .Do(call => logStore.Remove(call.Arg<TourLog>()));

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var expected = TourMetricsCalculator.Calculate(new[] { remainingLog });

            var useCase = new DeleteTourLogUseCase(tours, tourLogs, currentUser, unitOfWork);
            await useCase.ExecuteAsync(new DeleteTourLogRequest(deletedLog.Id));

            tourLogs.Received(1).Remove(deletedLog);
            Assert.That(tour.Popularity, Is.EqualTo(expected.Popularity));
            Assert.That(tour.ChildFriendliness, Is.EqualTo(expected.ChildFriendliness).Within(0.0001));
            await unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task TourSearchUseCase_ReturnsMatchingToursAndLogs()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Vienna loop", popularity: 4, childFriendliness: 75);
            var log = CreateLog(tour.Id, "Loved Vienna views", TourDifficulty.Medium, 15, 80, 4);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<Tour>)new[] { tour }.ToList()));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<TourLog>)new[] { log }.ToList()));

            var useCase = new TourSearchUseCase(tours, tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new TourSearchRequest("vienna"));

            Assert.That(result.Tours, Has.Count.EqualTo(1));
            Assert.That(result.TourLogs, Has.Count.EqualTo(1));
            Assert.That(result.Tours[0].Name, Is.EqualTo("Vienna loop"));
            Assert.That(result.TourLogs[0].Comment, Is.EqualTo("Loved Vienna views"));
        }
    }

    [TestFixture]
    public sealed class TourImportExportTests
    {
        [Test]
        public async Task ExportToursUseCase_SerializesCurrentUsersTours()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var tour = CreateTour(user.Id, "Exported tour", popularity: 3, childFriendliness: 88);
            var log = CreateLog(tour.Id, "Exported log", TourDifficulty.Easy, 9, 40, 5);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<Tour>)new[] { tour }.ToList()));

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.GetByTourIdAsync(tour.Id, user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<TourLog>)new[] { log }.ToList()));

            var useCase = new ExportToursUseCase(tours, tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new EmptyRequest());

            Assert.That(result.ContentType, Is.EqualTo("application/json"));
            Assert.That(result.FileName, Does.StartWith("tour-planner-export-").And.EndsWith(".json"));

            var exported = JsonSerializer.Deserialize<List<TourDetailResponseDto>>(Encoding.UTF8.GetString(result.Content), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.That(exported, Is.Not.Null);
            Assert.That(exported, Has.Count.EqualTo(1));
            Assert.That(exported![0].Name, Is.EqualTo("Exported tour"));
            Assert.That(exported[0].Logs, Has.Count.EqualTo(1));
            Assert.That(exported[0].Logs[0].Comment, Is.EqualTo("Exported log"));
        }

        [Test]
        public async Task ExportToursUseCase_ReturnsEmptyJsonWhenUserHasNoTours()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);

            var tours = Substitute.For<ITourRepository>();
            tours.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyList<Tour>)Array.Empty<Tour>()));

            var tourLogs = Substitute.For<ITourLogRepository>();

            var useCase = new ExportToursUseCase(tours, tourLogs, currentUser);
            var result = await useCase.ExecuteAsync(new EmptyRequest());

            Assert.That(result.ContentType, Is.EqualTo("application/json"));
            Assert.That(result.FileName, Does.StartWith("tour-planner-export-").And.EndsWith(".json"));

            var exported = JsonSerializer.Deserialize<List<TourDetailResponseDto>>(Encoding.UTF8.GetString(result.Content), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.That(exported, Is.Not.Null);
            Assert.That(exported, Is.Empty);
            await tourLogs.DidNotReceive().GetByTourIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ImportToursUseCase_ImportsToursAndLogs()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var importPayload = new List<TourDetailResponseDto>
            {
                new(
                    Guid.NewGuid(),
                    "Imported tour",
                    "Imported description",
                    "Vienna",
                    "Graz",
                    DomainTransportType.Bicycling,
                    12.5,
                    45,
                    "route-json",
                    7,
                    82,
                    null,
                    new[]
                    {
                        new TourLogResponseDto(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Parse("2026-07-01T10:00:00Z"), "Imported log", TourDifficulty.Medium, 12, 45, 4)
                    },
                    null)
            };

            var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(importPayload, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            var tours = Substitute.For<ITourRepository>();
            tours.AddAsync(Arg.Any<Tour>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var tourLogs = Substitute.For<ITourLogRepository>();
            tourLogs.AddAsync(Arg.Any<TourLog>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new ImportToursUseCase(tours, tourLogs, currentUser, unitOfWork);
            var result = await useCase.ExecuteAsync(new ImportToursRequest("import.json", content));

            Assert.That(result.ImportedTours, Is.EqualTo(1));
            Assert.That(result.ImportedTourLogs, Is.EqualTo(1));
            await tours.Received(1).AddAsync(Arg.Is<Tour>(tour => tour.UserId == user.Id && tour.Name == "Imported tour"), Arg.Any<CancellationToken>());
            await tourLogs.Received(1).AddAsync(Arg.Is<TourLog>(log => log.Comment == "Imported log"), Arg.Any<CancellationToken>());
            await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ImportToursUseCase_ImportsTourWithoutLogs()
        {
            var user = CreateUser();
            var currentUser = CreateCurrentUser(user);
            var importPayload = new List<TourDetailResponseDto>
            {
                new(
                    Guid.NewGuid(),
                    "Imported tour without logs",
                    "Imported description",
                    "Vienna",
                    "Graz",
                    DomainTransportType.Hiking,
                    18.2,
                    95,
                    "route-json",
                    0,
                    100,
                    null,
                    Array.Empty<TourLogResponseDto>(),
                    null)
            };

            var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(importPayload, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            var tours = Substitute.For<ITourRepository>();
            tours.AddAsync(Arg.Any<Tour>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var tourLogs = Substitute.For<ITourLogRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var useCase = new ImportToursUseCase(tours, tourLogs, currentUser, unitOfWork);
            var result = await useCase.ExecuteAsync(new ImportToursRequest("import.json", content));

            Assert.That(result.ImportedTours, Is.EqualTo(1));
            Assert.That(result.ImportedTourLogs, Is.EqualTo(0));
            await tours.Received(1).AddAsync(Arg.Is<Tour>(tour => tour.UserId == user.Id && tour.Name == "Imported tour without logs"), Arg.Any<CancellationToken>());
            await tourLogs.DidNotReceive().AddAsync(Arg.Any<TourLog>(), Arg.Any<CancellationToken>());
            await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    private static User CreateUser(string userName = "alice") => User.Create(userName, "hash");

    private static ICurrentUserContext CreateCurrentUser(User user)
    {
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(user.Id);
        currentUser.UserName.Returns(user.UserName);
        return currentUser;
    }

    private static Tour CreateTour(Guid userId, string name, int popularity, double childFriendliness, string? imagePath = null)
    {
        var tour = Tour.Create(
            userId,
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
        tour.UpdateImagePath(imagePath);
        return tour;
    }

    private static TourLog CreateLog(Guid tourId, string comment, TourDifficulty difficulty, double distance, double time, int rating)
        => TourLog.Create(tourId, DateTimeOffset.Parse("2026-07-01T08:00:00Z"), comment, difficulty, distance, time, rating);
}
