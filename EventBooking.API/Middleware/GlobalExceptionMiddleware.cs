using EventBooking.API.Common;
using EventBooking.Application.Exceptions;
using EventBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace EventBooking.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing.");

            // TODO: Implement custom response structure using ApiResponse<T> in future sprint
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case EmailAlreadyExistsException e:
                statusCode = HttpStatusCode.Conflict;
                message = e.Message;
                break;
            case InvalidCredentialsException e:
                statusCode = HttpStatusCode.Unauthorized;
                message = e.Message;
                break;
            case DomainException e:
                statusCode = HttpStatusCode.BadRequest;
                message = e.Message;
                break;
            case FluentValidation.ValidationException e:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                errors = e.Errors.Select(x => x.ErrorMessage).ToList();
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        var response = ApiResponse<object>.Failure(message, errors);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}
