using System.Net;
using System.Text.Json;
using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.Exceptions;

namespace DirectPayGateway.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        var traceId = context.TraceIdentifier;
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ApiResponse
        {
            Success = false,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = new ErrorDetails
                {
                    Code = validationEx.ErrorCode,
                    Message = validationEx.Message,
                    ValidationErrors = validationEx.Errors
                };
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error = new ErrorDetails
                {
                    Code = unauthorizedEx.ErrorCode,
                    Message = unauthorizedEx.Message
                };
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Error = new ErrorDetails
                {
                    Code = forbiddenEx.ErrorCode,
                    Message = forbiddenEx.Message
                };
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = new ErrorDetails
                {
                    Code = notFoundEx.ErrorCode,
                    Message = notFoundEx.Message
                };
                break;

            case PaymentException paymentEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse.Error = new ErrorDetails
                {
                    Code = paymentEx.ErrorCode,
                    Message = paymentEx.Message,
                    Details = paymentEx.DirectPayErrorCode?.ToString()
                };
                break;

            case ExternalServiceException externalEx:
                response.StatusCode = (int)HttpStatusCode.BadGateway;
                errorResponse.Error = new ErrorDetails
                {
                    Code = externalEx.ErrorCode,
                    Message = externalEx.Message,
                    Details = _environment.IsDevelopment() ? externalEx.ServiceName : null
                };
                _logger.LogError(exception, "External service error: {ServiceName}", externalEx.ServiceName);
                break;

            case RateLimitException rateLimitEx:
                response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                errorResponse.Error = new ErrorDetails
                {
                    Code = rateLimitEx.ErrorCode,
                    Message = rateLimitEx.Message
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = new ErrorDetails
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred. Please try again later.",
                    Details = _environment.IsDevelopment() ? exception.ToString() : null
                };
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}
