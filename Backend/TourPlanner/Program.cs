using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TourPlanner.API;
using TourPlanner.Application;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Options;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Middleware;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. SYSTEM LOGGING & CROSS-CUTTING CONFIGURATIONS
// =========================================================================
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AssemblyReference).Assembly)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Native .NET OpenAPI specifications engine
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// =========================================================================
// 2. CLEAN ARCHITECTURE LAYER REGISTRATIONS
// =========================================================================
builder.Services.AddTourPlannerApplication();
builder.Services.AddTourPlannerInfrastructure(builder.Configuration);

// =========================================================================
// 3. AUTHENTICATION & SECURITY CONFIGURATIONS
// =========================================================================
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException("Jwt:SigningKey must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

const string corsPolicy = "AllowAngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =========================================================================
// 4. PIPELINE EXECUTION (MIDDLEWARE CHAIN)
// =========================================================================
var app = builder.Build();

// Extracting your custom global error payload transformer into a clean separate method/middleware
app.UseTourPlannerExceptionHandler();

// Environment-specific feature branches
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options.WithTitle("TourPlanner API Explorer")
            .WithTheme(ScalarTheme.DeepSpace);
    }); // This maps to "/scalar/v1"
}

// Database migrations execution (Controlled via environment setup)
if (!app.Environment.IsProduction() && !builder.Configuration.GetValue<bool>("DisableDatabaseMigrations")) {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TourPlannerDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }