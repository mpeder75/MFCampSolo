using System.Net;
using System.Text.Json;
using Order.Domain.Exceptions;

namespace Order.API.Middelware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");
        
        var statusCode = HttpStatusCode.InternalServerError;
        var errorMessage = "An unexpected error occurred";
        
        statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            DomainException => HttpStatusCode.BadRequest
        };

        if (exception.GetType().Name.Contains("NotFound"))
        {
            statusCode = HttpStatusCode.NotFound;
        }

        errorMessage = exception.Message;
        
        var response = new
        {
            error = errorMessage,
            statusCode = (int)statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}