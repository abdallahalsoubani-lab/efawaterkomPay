using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/admin/ctm-api-logs")]
[AllowAnonymous]
public class CtmApiLogsController : ControllerBase
{
    private readonly ICtmLoggingService _loggingService;

    public CtmApiLogsController(ICtmLoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CtmApiLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] CtmApiLogQueryFilter filter)
    {
        var result = await _loggingService.GetLogsPagedAsync(filter);

        var dtos = new PagedResult<CtmApiLogDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<CtmApiLogDto>>.Ok(dtos));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CtmApiLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLogById(int id)
    {
        var result = await _loggingService.GetLogsPagedAsync(new CtmApiLogQueryFilter { Page = 1, PageSize = 1 });
        var log = result.Items.FirstOrDefault(l => l.Id == id);

        if (log == null)
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Log entry not found"));

        return Ok(ApiResponse<CtmApiLogDto>.Ok(MapToDto(log)));
    }

    private static CtmApiLogDto MapToDto(CtmApiLog log) => new()
    {
        Id = log.Id,
        Timestamp = log.Timestamp,
        Endpoint = log.Endpoint,
        HttpMethod = log.HttpMethod,
        RequestBody = log.RequestBody,
        ResponseBody = log.ResponseBody,
        HttpStatusCode = log.HttpStatusCode,
        BillingNo = log.BillingNo,
        JOEBPPSTrx = log.JOEBPPSTrx,
        ErrorMessage = log.ErrorMessage,
        ClientIp = log.ClientIp,
        UserAgent = log.UserAgent,
        ResponseTimeMs = log.ResponseTimeMs
    };
}

public class CtmApiLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public int HttpStatusCode { get; set; }
    public string? BillingNo { get; set; }
    public string? JOEBPPSTrx { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public long ResponseTimeMs { get; set; }
}
