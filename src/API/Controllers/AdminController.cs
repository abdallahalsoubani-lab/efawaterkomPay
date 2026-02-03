using System.Globalization;
using System.Text;
using CsvHelper;
using DirectPayGateway.API.Extensions;
using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.DTOs.Admin;
using DirectPayGateway.Core.DTOs.Auth;
using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using DirectPayGateway.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IUserRepository _userRepository;
    private readonly IStatisticsService _statisticsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        PaymentService paymentService,
        IUserRepository userRepository,
        IStatisticsService statisticsService,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger)
    {
        _paymentService = paymentService;
        _userRepository = userRepository;
        _statisticsService = statisticsService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatistics>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var statistics = await _statisticsService.GetDashboardStatisticsAsync(fromDate, toDate);
        return Ok(ApiResponse<DashboardStatistics>.Ok(statistics));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterRequest filter)
    {
        var result = await _paymentService.GetAllTransactionsAsync(filter);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.Ok(result));
    }

    [HttpGet("transactions/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTransactions([FromQuery] ExportRequest request)
    {
        var filter = new TransactionFilterRequest
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Page = 1,
            PageSize = 10000
        };

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<PaymentStatus>(request.Status, out var status))
        {
            filter.Status = status;
        }

        var result = await _paymentService.GetAllTransactionsAsync(filter);

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(result.Items.Select(t => new
        {
            t.Id,
            t.BillerTrxNo,
            t.DirectPayTrxNo,
            t.UserEmail,
            t.Amount,
            t.Currency,
            t.BillingNo,
            t.PaymentTypeName,
            t.StatusName,
            t.TrxStatusCode,
            t.TrxStatusMessage,
            CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            CompletedAt = t.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
        }));

        await writer.FlushAsync();
        var bytes = memoryStream.ToArray();

        var fileName = $"transactions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserListRequest filter)
    {
        var result = await _userRepository.GetPagedAsync(filter);

        var userDtos = new List<UserDetailDto>();
        foreach (var user in result.Items)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var transactions = user.Transactions ?? new List<PaymentTransaction>();

            userDtos.Add(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList(),
                TransactionCount = transactions.Count,
                TotalTransactionAmount = transactions
                    .Where(t => t.Status == PaymentStatus.Success)
                    .Sum(t => t.Amount)
            });
        }

        var pagedResult = new PagedResult<UserDetailDto>
        {
            Items = userDtos,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<UserDetailDto>>.Ok(pagedResult));
    }

    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "User not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var transactions = user.Transactions ?? new List<PaymentTransaction>();

        var userDto = new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList(),
            TransactionCount = transactions.Count,
            TotalTransactionAmount = transactions
                .Where(t => t.Status == PaymentStatus.Success)
                .Sum(t => t.Amount)
        };

        return Ok(ApiResponse<UserDetailDto>.Ok(userDto));
    }

    [HttpPut("users/{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "User not found"));
        }

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse.Fail("UPDATE_FAILED",
                string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var userDto = new UserDto
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

        _logger.LogInformation("User updated by admin: {UserId}", id);

        return Ok(ApiResponse<UserDto>.Ok(userDto, "User updated successfully"));
    }

    [HttpPut("users/{id}/roles")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "User not found"));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var validRoles = request.Roles.Where(r => Roles.All.Contains(r)).ToList();
        await _userManager.AddToRolesAsync(user, validRoles);

        _logger.LogInformation("User roles updated by admin: {UserId}, Roles: {Roles}",
            id, string.Join(", ", validRoles));

        return Ok(ApiResponse.Ok("User roles updated successfully"));
    }
}
