using FluentValidation;
using TourPlanner.Application.Common.Exceptions;

namespace TourPlanner.Middleware;

internal static class ExceptionHandlerExtensions {
    public static IApplicationBuilder UseTourPlannerExceptionHandler(this IApplicationBuilder app) {
        return app.Use(async (context, next) => {
            try {
                await next();
            }
            catch (Exception exception) {
                if (context.Response.HasStarted) {
                    throw;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = GetStatusCode(exception);

                var payload = GetErrorPayload(exception);
                await context.Response.WriteAsJsonAsync(payload);
            }
        });
    }

    private static int GetStatusCode(Exception exception) => exception switch {
        TourPlannerValidationException or ValidationException => StatusCodes.Status400BadRequest,
        TourPlannerUnauthorizedException => StatusCodes.Status401Unauthorized,
        TourPlannerNotFoundException or KeyNotFoundException => StatusCodes.Status404NotFound,
        TourPlannerConflictException => StatusCodes.Status409Conflict,
        ArgumentException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };
    
    private static object GetErrorPayload(Exception exception) => exception switch {
        ValidationException valEx => new {
            error = "Validation failed.",
            errors = valEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        },
        TourPlannerException tpEx => new { error = tpEx.Message},
        _ => new { error = exception.Message }
    };
}