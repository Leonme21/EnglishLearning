using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnglishLearning.Domain.Common;

namespace EnglishLearning.Api.Filters;

public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new
                {
                    field = e.Key,
                    message = err.ErrorMessage
                }));

            var errorResponse = new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Datos de entrada inválidos",
                    details = errors
                }
            };

            context.Result = new BadRequestObjectResult(errorResponse);
            return;
        }

        await next();
    }
}

public sealed class ApiResponseEnvelopeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            // Only wrap successful responses (2xx)
            if (objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
            {
                var envelope = new
                {
                    success = true,
                    data = objectResult.Value,
                    correlationId = context.HttpContext.Items["CorrelationId"]?.ToString()
                };
                context.Result = new ObjectResult(envelope) { StatusCode = objectResult.StatusCode };
            }
        }

        await next();
    }
}