using EventBooking.API.Extensions;
using EventBooking.Application.Extensions;
using EventBooking.Application.Interfaces.Auth;
using EventBooking.Infrastructure.Extensions;
using EventBooking.Infrastructure.Persistence;
using EventBooking.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPresentation(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

        var app = builder.Build();

        // Apply pending migrations and seed dev data automatically in Development only.
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            await context.Database.MigrateAsync();
            await DataSeeder.SeedAsync(context, passwordHasher);
        }

        app.UsePresentation();

        await app.RunAsync();
    }
}