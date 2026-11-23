using EventBooking.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace EventBooking.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            // Register our custom validation filter globally
            options.Filters.Add<ValidationFilter>();
        })
        .AddJsonOptions(options =>
         {
             options.JsonSerializerOptions.Converters.Add(
                 new System.Text.Json.Serialization.JsonStringEnumConverter()
             );
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

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token directly below."
            });

            options.AddSecurityRequirement(document =>
            {
                var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
                return new OpenApiSecurityRequirement
                {
                    [schemeReference] = new List<string>()
                };
            });
        });

        // Configure Health Check Endpoint Infrastructure
        services.AddHealthChecks();

        // CORS: lets the (not-yet-built) React frontend call this API from allowed local
        // dev origins. Origins come from configuration ("Cors:AllowedOrigins") rather
        // than being hardcoded; if the section is missing, this falls back to an empty
        // origin list instead of throwing at startup. AllowAnyOrigin()/wildcard is
        // intentionally never used, and AllowCredentials() is intentionally omitted —
        // this API authenticates via the Authorization header (JWT), not cookies, so
        // credentialed CORS brings no benefit and would only tighten what origin
        // configurations are legal.
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }
}