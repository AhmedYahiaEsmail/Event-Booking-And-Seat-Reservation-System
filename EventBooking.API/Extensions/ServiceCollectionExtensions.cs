using EventBooking.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace EventBooking.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Register our custom validation filter globally
            options.Filters.Add<ValidationFilter>();
        });

        // Suppress default model state validation so our ValidationFilter handles it consistently
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddEndpointsApiExplorer();

        // Configure Swagger
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Event Booking API",
                Version = "v1",
                Description = "Event Booking & Seat Reservation System API"
            });
        });

        // Configure Health Check Endpoint Infrastructure
        services.AddHealthChecks();

        return services;
    }
}
