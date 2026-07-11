using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Behaviors;

namespace TourPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTourPlannerApplication(this IServiceCollection services) {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCase<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCase<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );
        
        services.Decorate(typeof(IUseCase<,>), typeof(UseCaseValidatorDecorator<,>));
        services.Decorate(typeof(IUseCase<>), typeof(UseCaseValidatorDecorator<>));
        
        services.Decorate(typeof(IUseCase<,>), typeof(UseCaseLoggingDecorator<,>));
        services.TryDecorate(typeof(IUseCase<>), typeof(UseCaseLoggingDecorator<>));
        
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}



