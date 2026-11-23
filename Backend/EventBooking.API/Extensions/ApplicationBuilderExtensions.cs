using EventBooking.API.Middleware;

namespace EventBooking.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Booking API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // Global Exception Middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // CORS must run before Authentication/Authorization so preflight (OPTIONS)
        // requests and cross-origin responses are handled before the auth pipeline
        // gets involved.
        app.UseCors("FrontendPolicy");
        app.UseHttpsRedirection();

        // Authentication & Authorization Middleware Pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Health Check Endpoint
        app.MapHealthChecks("/health");

        return app;
    }
}