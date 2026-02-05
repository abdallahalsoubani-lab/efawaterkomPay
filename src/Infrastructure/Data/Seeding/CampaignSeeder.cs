using System.Text.Json;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectPayGateway.Infrastructure.Data.Seeding;

public class CampaignSeeder
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CampaignSeeder> _logger;

    public CampaignSeeder(
        ICampaignRepository campaignRepository,
        IConfiguration configuration,
        ILogger<CampaignSeeder> logger)
    {
        _campaignRepository = campaignRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var count = await _campaignRepository.GetCountAsync();
        if (count > 0)
        {
            _logger.LogInformation("Campaigns already exist ({Count}). Skipping seed.", count);
            return;
        }

        var seedFile = _configuration["CtmIntegration:CampaignSeedFile"] ?? "campaigns-seed.json";

        if (!File.Exists(seedFile))
        {
            _logger.LogWarning("Campaign seed file not found: {SeedFile}", seedFile);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(seedFile);
            var seedData = JsonSerializer.Deserialize<CampaignSeedData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedData?.Campaigns == null || seedData.Campaigns.Count == 0)
            {
                _logger.LogWarning("No campaigns found in seed file");
                return;
            }

            var defaultIban = seedData.GlobalSettings?.DefaultIBAN ?? "";
            var defaultBankCode = seedData.GlobalSettings?.DefaultBankCode ?? "";

            var campaigns = seedData.Campaigns.Select(c => new Campaign
            {
                CampaignCode = c.CampaignCode,
                BillNo = c.BillNo,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                DescriptionAr = c.DescriptionAr,
                DescriptionEn = c.DescriptionEn,
                Category = c.Category ?? "",
                ServiceType = c.ServiceType ?? seedData.GlobalSettings?.DefaultServiceType ?? "Donations",
                Status = c.Status ?? "Active",
                BillType = c.BillType ?? "Recurring",
                BillCustomerCat = c.BillCustomerCat ?? "Citizen",
                TargetAmount = c.TargetAmount,
                CollectedAmount = c.CollectedAmount,
                DonorsCount = c.DonorsCount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                AllowPartialPayment = c.PaymentConstraints?.AllowPartialPayment ?? true,
                MinAmount = c.PaymentConstraints?.MinAmount ?? 1.000m,
                MaxAmount = c.PaymentConstraints?.MaxAmount ?? 100000.000m,
                IBAN = c.BankAccount?.Iban ?? defaultIban,
                BankCode = c.BankAccount?.BankCode ?? defaultBankCode,
                CustName = c.AdditionalInfo?.CustName,
                FreeText = c.AdditionalInfo?.FreeText,
                Email = c.AdditionalInfo?.Email,
                Phone = c.AdditionalInfo?.Phone,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _campaignRepository.BulkInsertAsync(campaigns);

            _logger.LogInformation("Seeded {Count} campaigns from {SeedFile}",
                campaigns.Count, seedFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed campaigns from {SeedFile}", seedFile);
        }
    }
}

// Seed file models
public class CampaignSeedData
{
    public GlobalSettings? GlobalSettings { get; set; }
    public List<CampaignSeedItem> Campaigns { get; set; } = new();
}

public class GlobalSettings
{
    public string? BillerCode { get; set; }
    public string? SdrCode { get; set; }
    public string? DefaultIBAN { get; set; }
    public string? DefaultBankCode { get; set; }
    public string? Currency { get; set; }
    public string? DefaultServiceType { get; set; }
    public BasicAuthConfig? BasicAuth { get; set; }
}

public class BasicAuthConfig
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class CampaignSeedItem
{
    public string CampaignCode { get; set; } = string.Empty;
    public string BillNo { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? Category { get; set; }
    public string? ServiceType { get; set; }
    public string? Status { get; set; }
    public string? BillType { get; set; }
    public string? BillCustomerCat { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public int DonorsCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PaymentConstraintsConfig? PaymentConstraints { get; set; }
    public BankAccountConfig? BankAccount { get; set; }
    public AdditionalInfoConfig? AdditionalInfo { get; set; }
}

public class PaymentConstraintsConfig
{
    public bool AllowPartialPayment { get; set; } = true;
    public decimal MinAmount { get; set; } = 1.000m;
    public decimal MaxAmount { get; set; } = 100000.000m;
}

public class BankAccountConfig
{
    public string Iban { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
}

public class AdditionalInfoConfig
{
    public string? CustName { get; set; }
    public string? FreeText { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
