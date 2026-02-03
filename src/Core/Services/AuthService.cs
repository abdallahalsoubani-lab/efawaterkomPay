using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.DTOs.Auth;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Exceptions;
using DirectPayGateway.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DirectPayGateway.Core.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User with this email already exists"
            };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, errors);
            return new AuthResponse
            {
                Success = false,
                Message = errors
            };
        }

        await _userManager.AddToRoleAsync(user, Roles.User);

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        var token = await _tokenService.GenerateJwtTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed - user inactive: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Your account has been deactivated"
            };
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            _logger.LogWarning("Login failed - invalid password: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await _tokenService.GenerateJwtTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

        _logger.LogInformation("User logged in: {Email}", request.Email);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var storedToken = await _tokenService.GetRefreshTokenAsync(refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid or expired refresh token"
            };
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);

        if (user == null || !user.IsActive)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User not found or inactive"
            };
        }

        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress, newRefreshToken.Token);

        var jwtToken = await _tokenService.GenerateJwtTokenAsync(user);

        _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

        return new AuthResponse
        {
            Success = true,
            Token = jwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt,
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive)
            return null;

        return await MapToUserDtoAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User", userId);

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }

        await _tokenService.RevokeAllUserRefreshTokensAsync(userId);

        _logger.LogInformation("Password changed for user: {Email}", user.Email);
        return true;
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress = null)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);
    }

    private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList()
        };
    }
}
