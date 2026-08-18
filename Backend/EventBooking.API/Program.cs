using EventBooking.API.Extensions;
using EventBooking.Application.Extensions;
using EventBooking.Infrastructure.Extensions;

namespace EventBooking.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register Services (Presentation, Application, Infrastructure)
        builder.Services.AddPresentation(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

        var app = builder.Build();

        // Configure HTTP Request Pipeline
        app.UsePresentation();

        app.Run();
    }
}