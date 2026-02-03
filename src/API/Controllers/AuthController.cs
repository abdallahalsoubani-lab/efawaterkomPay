using DirectPayGateway.API.Extensions;
using DirectPayGateway.Core.DTOs.Auth;
using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var ipAddress = HttpContext.GetClientIpAddress();
        var result = await _authService.RegisterAsync(request, ipAddress);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("REGISTRATION_FAILED", result.Message ?? "Registration failed"));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful"));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.GetClientIpAddress();
        var result = await _authService.LoginAsync(request, ipAddress);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("LOGIN_FAILED", result.Message ?? "Login failed"));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.GetClientIpAddress();
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("REFRESH_FAILED", result.Message ?? "Token refresh failed"));
        }

        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "User not found"));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.Ok("Password changed successfully"));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.GetClientIpAddress();
        await _authService.LogoutAsync(request.RefreshToken, ipAddress);
        return Ok(ApiResponse.Ok("Logged out successfully"));
    }
}
