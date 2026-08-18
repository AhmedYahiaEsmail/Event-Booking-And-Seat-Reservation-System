using EventBooking.Application.Interfaces.Auth;
using EventBooking.Application.Interfaces.Events;
using EventBooking.Application.Interfaces.Reservations;
using EventBooking.Application.Services.Auth;
using EventBooking.Application.Services.Events;
using EventBooking.Application.Services.Reservations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBooking.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}