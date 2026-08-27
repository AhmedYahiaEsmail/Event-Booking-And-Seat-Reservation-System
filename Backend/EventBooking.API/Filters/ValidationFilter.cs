using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventBooking.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var arguments = context.ActionArguments.Values.Where(v => v != null);

        foreach (var argument in arguments)
        {
            if (argument == null) continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    // Throw the exception instead of manually formatting the HTTP response.
                    // The GlobalExceptionMiddleware will catch this and format it uniformly.
                    throw new ValidationException(validationResult.Errors);
                }
            }
        }

        await next();
    }
}