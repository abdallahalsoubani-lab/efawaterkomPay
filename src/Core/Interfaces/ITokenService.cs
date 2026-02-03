using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress = null);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null);
    Task RevokeAllUserRefreshTokensAsync(string userId, string? ipAddress = null);
}
