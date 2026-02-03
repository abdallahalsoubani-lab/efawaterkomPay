namespace DirectPayGateway.API.Extensions;

public static class HttpContextExtensions
{
    public static string? GetClientIpAddress(this HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    public static string? GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers["User-Agent"].FirstOrDefault();
    }
}
