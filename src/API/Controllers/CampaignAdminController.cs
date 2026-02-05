using System.Globalization;
using System.Text;
using CsvHelper;
using DirectPayGateway.Core.Constants;
using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Common;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectPayGateway.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class CampaignAdminController : ControllerBase
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IDonationRepository _donationRepository;
    private readonly ICtmAuditLogRepository _auditLogRepository;
    private readonly ILogger<CampaignAdminController> _logger;

    public CampaignAdminController(
        ICampaignRepository campaignRepository,
        IDonationRepository donationRepository,
        ICtmAuditLogRepository auditLogRepository,
        ILogger<CampaignAdminController> logger)
    {
        _campaignRepository = campaignRepository;
        _donationRepository = donationRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    // Campaign endpoints

    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaigns([FromQuery] CampaignFilterRequest filter)
    {
        var result = await _campaignRepository.GetPagedAsync(filter);

        var dtos = new PagedResult<CampaignDto>
        {
            Items = result.Items.Select(MapToCampaignDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<CampaignDto>>.Ok(dtos));
    }

    [HttpGet("campaigns/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampaign(int id)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Campaign not found"));
        }

        var donationFilter = new DonationFilterRequest
        {
            CampaignId = id,
            Page = 1,
            PageSize = 20,
            SortDescending = true
        };
        var donations = await _donationRepository.GetPagedAsync(donationFilter);

        var dto = new CampaignDetailDto
        {
            Id = campaign.Id,
            CampaignCode = campaign.CampaignCode,
            BillNo = campaign.BillNo,
            NameAr = campaign.NameAr,
            NameEn = campaign.NameEn,
            DescriptionAr = campaign.DescriptionAr,
            DescriptionEn = campaign.DescriptionEn,
            Category = campaign.Category,
            ServiceType = campaign.ServiceType,
            Status = campaign.Status,
            BillType = campaign.BillType,
            BillCustomerCat = campaign.BillCustomerCat,
            TargetAmount = campaign.TargetAmount,
            CollectedAmount = campaign.CollectedAmount,
            DonorsCount = campaign.DonorsCount,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            AllowPartialPayment = campaign.AllowPartialPayment,
            MinAmount = campaign.MinAmount,
            MaxAmount = campaign.MaxAmount,
            IBAN = campaign.IBAN,
            BankCode = campaign.BankCode,
            CustName = campaign.CustName,
            FreeText = campaign.FreeText,
            Email = campaign.Email,
            Phone = campaign.Phone,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            RecentDonations = donations.Items.Select(MapToDonationDto).ToList()
        };

        return Ok(ApiResponse<CampaignDetailDto>.Ok(dto));
    }

    [HttpPost("campaigns")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var existing = await _campaignRepository.GetByCampaignCodeAsync(request.CampaignCode);
        if (existing != null)
        {
            return BadRequest(ApiResponse.Fail("DUPLICATE",
                $"Campaign with code '{request.CampaignCode}' already exists"));
        }

        var campaign = new Campaign
        {
            CampaignCode = request.CampaignCode,
            BillNo = request.BillNo,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            Category = request.Category,
            ServiceType = request.ServiceType,
            Status = request.Status,
            BillType = request.BillType,
            BillCustomerCat = request.BillCustomerCat,
            TargetAmount = request.TargetAmount,
            CollectedAmount = 0,
            DonorsCount = 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AllowPartialPayment = request.AllowPartialPayment,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            IBAN = request.IBAN,
            BankCode = request.BankCode,
            CustName = request.CustName,
            FreeText = request.FreeText,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _campaignRepository.CreateAsync(campaign);

        _logger.LogInformation("Campaign created: {CampaignCode}", campaign.CampaignCode);

        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id },
            ApiResponse<CampaignDto>.Ok(MapToCampaignDto(campaign), "Campaign created successfully"));
    }

    [HttpPut("campaigns/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCampaign(int id, [FromBody] UpdateCampaignRequest request)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            return NotFound(ApiResponse.Fail("NOT_FOUND", "Campaign not found"));
        }

        if (request.NameAr != null) campaign.NameAr = request.NameAr;
        if (request.NameEn != null) campaign.NameEn = request.NameEn;
        if (request.DescriptionAr != null) campaign.DescriptionAr = request.DescriptionAr;
        if (request.DescriptionEn != null) campaign.DescriptionEn = request.DescriptionEn;
        if (request.Category != null) campaign.Category = request.Category;
        if (request.Status != null) campaign.Status = request.Status;
        if (request.TargetAmount.HasValue) campaign.TargetAmount = request.TargetAmount.Value;
        if (request.StartDate.HasValue) campaign.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) campaign.EndDate = request.EndDate.Value;
        if (request.AllowPartialPayment.HasValue) campaign.AllowPartialPayment = request.AllowPartialPayment.Value;
        if (request.MinAmount.HasValue) campaign.MinAmount = request.MinAmount.Value;
        if (request.MaxAmount.HasValue) campaign.MaxAmount = request.MaxAmount.Value;
        if (request.IBAN != null) campaign.IBAN = request.IBAN;
        if (request.BankCode != null) campaign.BankCode = request.BankCode;
        if (request.CustName != null) campaign.CustName = request.CustName;
        if (request.FreeText != null) campaign.FreeText = request.FreeText;
        if (request.Email != null) campaign.Email = request.Email;
        if (request.Phone != null) campaign.Phone = request.Phone;

        campaign.UpdatedAt = DateTime.UtcNow;
        await _campaignRepository.UpdateAsync(campaign);

        _logger.LogInformation("Campaign updated: {CampaignCode}", campaign.CampaignCode);

        return Ok(ApiResponse<CampaignDto>.Ok(MapToCampaignDto(campaign), "Campaign updated successfully"));
    }

    // Donation endpoints

    [HttpGet("donations")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DonationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDonations([FromQuery] DonationFilterRequest filter)
    {
        var result = await _donationRepository.GetPagedAsync(filter);

        var dtos = new PagedResult<DonationDto>
        {
            Items = result.Items.Select(MapToDonationDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<DonationDto>>.Ok(dtos));
    }

    [HttpGet("donations/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDonations(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? campaignId)
    {
        var filter = new DonationFilterRequest
        {
            FromDate = fromDate,
            ToDate = toDate,
            CampaignId = campaignId,
            Page = 1,
            PageSize = 50000,
            SortDescending = true
        };

        var result = await _donationRepository.GetPagedAsync(filter);

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(result.Items.Select(d => new
        {
            d.Id,
            d.JOEBPPSTrx,
            d.BankTrxId,
            CampaignCode = d.Campaign?.CampaignCode,
            CampaignName = d.Campaign?.NameEn,
            d.PaidAmount,
            d.FeesAmount,
            d.Currency,
            d.PmtStatus,
            d.AccessChannel,
            d.PaymentMethod,
            d.PayerIdType,
            d.PayerId,
            d.PayerNation,
            d.IsAcknowledged,
            ProcessDate = d.ProcessDate.ToString("yyyy-MM-dd HH:mm:ss"),
            CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        }));

        await writer.FlushAsync();
        var bytes = memoryStream.ToArray();

        var fileName = $"donations_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // CTM Audit Logs

    [HttpGet("ctm-logs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CtmAuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCtmLogs([FromQuery] CtmAuditLogFilterRequest filter)
    {
        var result = await _auditLogRepository.GetPagedAsync(filter);

        var dtos = new PagedResult<CtmAuditLogDto>
        {
            Items = result.Items.Select(l => new CtmAuditLogDto
            {
                Id = l.Id,
                ApiName = l.ApiName,
                GUID = l.GUID,
                RequestType = l.RequestType,
                RequestBody = l.RequestBody,
                ResponseBody = l.ResponseBody,
                ResponseCode = l.ResponseCode,
                IpAddress = l.IpAddress,
                Timestamp = l.Timestamp,
                DurationMs = l.DurationMs
            }).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<CtmAuditLogDto>>.Ok(dtos));
    }

    // Mapping helpers

    private static CampaignDto MapToCampaignDto(Campaign c)
    {
        return new CampaignDto
        {
            Id = c.Id,
            CampaignCode = c.CampaignCode,
            BillNo = c.BillNo,
            NameAr = c.NameAr,
            NameEn = c.NameEn,
            DescriptionAr = c.DescriptionAr,
            DescriptionEn = c.DescriptionEn,
            Category = c.Category,
            ServiceType = c.ServiceType,
            Status = c.Status,
            BillType = c.BillType,
            BillCustomerCat = c.BillCustomerCat,
            TargetAmount = c.TargetAmount,
            CollectedAmount = c.CollectedAmount,
            DonorsCount = c.DonorsCount,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            AllowPartialPayment = c.AllowPartialPayment,
            MinAmount = c.MinAmount,
            MaxAmount = c.MaxAmount,
            IBAN = c.IBAN,
            BankCode = c.BankCode,
            CustName = c.CustName,
            FreeText = c.FreeText,
            Email = c.Email,
            Phone = c.Phone,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    private static DonationDto MapToDonationDto(Donation d)
    {
        return new DonationDto
        {
            Id = d.Id,
            CampaignId = d.CampaignId,
            CampaignName = d.Campaign?.NameEn,
            CampaignCode = d.Campaign?.CampaignCode,
            JOEBPPSTrx = d.JOEBPPSTrx,
            BankTrxId = d.BankTrxId,
            BankCode = d.BankCode,
            BillingNo = d.BillingNo,
            DueAmount = d.DueAmount,
            PaidAmount = d.PaidAmount,
            FeesAmount = d.FeesAmount,
            FeesOnBiller = d.FeesOnBiller,
            PmtStatus = d.PmtStatus,
            Currency = d.Currency,
            AccessChannel = d.AccessChannel,
            PaymentMethod = d.PaymentMethod,
            PaymentType = d.PaymentType,
            ProcessDate = d.ProcessDate,
            StmtDate = d.StmtDate,
            PayerIdType = d.PayerIdType,
            PayerId = d.PayerId,
            PayerNation = d.PayerNation,
            PayerName = d.PayerName,
            PayerPhone = d.PayerPhone,
            PayerEmail = d.PayerEmail,
            IsAcknowledged = d.IsAcknowledged,
            CreatedAt = d.CreatedAt
        };
    }
}
