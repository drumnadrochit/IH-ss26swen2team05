using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class TourPlannerDbContext(DbContextOptions<TourPlannerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Tour> Tours => Set<Tour>();

    public DbSet<TourLog> TourLogs => Set<TourLog>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.UserName).IsUnique();
            entity.Property(user => user.UserName).HasMaxLength(128).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.HasMany(user => user.Tours).WithOne(tour => tour.User).HasForeignKey(tour => tour.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(user => user.Sessions).WithOne(session => session.User).HasForeignKey(session => session.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.HasIndex(session => session.RefreshToken).IsUnique();
            entity.Property(session => session.RefreshToken).HasMaxLength(512).IsRequired();
            entity.Property(session => session.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(tour => tour.Id);
            entity.Property(tour => tour.Name).HasMaxLength(256).IsRequired();
            entity.Property(tour => tour.Description).HasMaxLength(4000).IsRequired();
            entity.Property(tour => tour.From).HasMaxLength(256).IsRequired();
            entity.Property(tour => tour.To).HasMaxLength(256).IsRequired();
            entity.Property(tour => tour.RouteInformation).HasColumnType("text").IsRequired();
            entity.Property(tour => tour.ImagePath).HasMaxLength(1024);
            entity.Property(tour => tour.TransportType).HasConversion<string>().HasMaxLength(64);
            entity.Property(tour => tour.DistanceKm).HasPrecision(18, 2);
            entity.Property(tour => tour.EstimatedMinutes).HasPrecision(18, 2);
            entity.Property(tour => tour.ChildFriendliness).HasPrecision(18, 2);
            entity.HasMany(tour => tour.TourLogs).WithOne(log => log.Tour).HasForeignKey(log => log.TourId).OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(tour => tour.RouteGeoJson)
                .HasColumnType("jsonb")
                .IsRequired(false);

            entity.OwnsOne(tour => tour.FromLocation, navigation =>
            {
                navigation.Property(coords => coords.Latitude).HasColumnName("FromLatitude");
                navigation.Property(coords => coords.Longitude).HasColumnName("FromLongitude");
            });

            entity.OwnsOne(tour => tour.ToLocation, navigation =>
            {
                navigation.Property(coords => coords.Latitude).HasColumnName("ToLatitude");
                navigation.Property(coords => coords.Longitude).HasColumnName("ToLongitude");
            });
        });

        modelBuilder.Entity<TourLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Comment).HasMaxLength(4000).IsRequired();
            entity.Property(log => log.Difficulty).HasConversion<string>().HasMaxLength(32);
            entity.Property(log => log.TotalDistanceKm).HasPrecision(18, 2);
            entity.Property(log => log.TotalTimeMinutes).HasPrecision(18, 2);
            entity.Property(log => log.Rating).IsRequired();
            entity.HasIndex(log => log.TourId);
        });
    }
}

