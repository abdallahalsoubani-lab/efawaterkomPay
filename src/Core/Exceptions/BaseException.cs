using System.Net;

namespace DirectPayGateway.Core.Exceptions;

public abstract class BaseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }

    protected BaseException(string message, HttpStatusCode statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    protected BaseException(string message, HttpStatusCode statusCode, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message, HttpStatusCode.BadRequest, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest, "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, HttpStatusCode.Unauthorized, "UNAUTHORIZED")
    {
    }
}

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, HttpStatusCode.Forbidden, "FORBIDDEN")
    {
    }
}

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound, "NOT_FOUND")
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.", HttpStatusCode.NotFound, "NOT_FOUND")
    {
    }
}

public class PaymentException : BaseException
{
    public int? DirectPayErrorCode { get; }

    public PaymentException(string message, int? directPayErrorCode = null)
        : base(message, HttpStatusCode.UnprocessableEntity, "PAYMENT_FAILED")
    {
        DirectPayErrorCode = directPayErrorCode;
    }
}

public class ExternalServiceException : BaseException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base(message, HttpStatusCode.BadGateway, "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base(message, HttpStatusCode.BadGateway, "EXTERNAL_SERVICE_ERROR", innerException)
    {
        ServiceName = serviceName;
    }
}

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict, "CONFLICT")
    {
    }
}

public class RateLimitException : BaseException
{
    public RateLimitException(string message = "Too many requests. Please try again later.")
        : base(message, HttpStatusCode.TooManyRequests, "RATE_LIMITED")
    {
    }
}
