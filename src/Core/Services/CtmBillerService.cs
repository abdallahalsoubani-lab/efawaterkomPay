using DirectPayGateway.Core.DTOs.Ctm;
using DirectPayGateway.Core.DTOs.Ctm.Shared;
using DirectPayGateway.Core.Entities;
using DirectPayGateway.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DirectPayGateway.Core.Services;

public class CtmBillerService : ICtmBillerService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IDonationRepository _donationRepository;
    private readonly ILogger<CtmBillerService> _logger;
    private readonly string _billerCode;

    public CtmBillerService(
        ICampaignRepository campaignRepository,
        IDonationRepository donationRepository,
        IConfiguration configuration,
        ILogger<CtmBillerService> logger)
    {
        _campaignRepository = campaignRepository;
        _donationRepository = donationRepository;
        _logger = logger;
        _billerCode = configuration["CtmIntegration:BillerCode"] ?? "000";
    }

    public async Task<MfepBillPullResponse> HandleBillPullAsync(MfepBillPullRequest request)
    {
        var guid = request.MFEP.MsgHeader.GUID;
        var billingNo = request.MFEP.MsgBody.AcctInfo.BillingNo;

        _logger.LogInformation("BillPull request for BillingNo: {BillingNo}, GUID: {GUID}", billingNo, guid);

        var campaign = await _campaignRepository.GetByCampaignCodeAsync(billingNo);

        if (campaign == null)
        {
            _logger.LogWarning("BillPull: Campaign not found for BillingNo: {BillingNo}", billingNo);
            return BuildBillPullErrorResponse(guid, 1, "Campaign not found");
        }

        if (campaign.Status != "Active")
        {
            _logger.LogWarning("BillPull: Campaign not active. Code: {Code}, Status: {Status}",
                campaign.CampaignCode, campaign.Status);
            return BuildBillPullErrorResponse(guid, 2, "Campaign expired or closed");
        }

        if (DateTime.UtcNow > campaign.EndDate)
        {
            _logger.LogWarning("BillPull: Campaign expired. Code: {Code}, EndDate: {EndDate}",
                campaign.CampaignCode, campaign.EndDate);
            return BuildBillPullErrorResponse(guid, 2, "Campaign expired or closed");
        }

        var response = new MfepBillPullResponse
        {
            MFEP = new BillPullMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "BILPULRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = 0,
                        ErrorDesc = "Success",
                        Severity = "Info"
                    }
                },
                MsgBody = new BillPullResponseBody
                {
                    RecCount = 1,
                    BillRec = new List<BillRecord>
                    {
                        new BillRecord
                        {
                            Result = new MfepResult
                            {
                                ErrorCode = 0,
                                ErrorDesc = "Success",
                                Severity = "Info"
                            },
                            AcctInfo = new AcctInfo
                            {
                                BillingNo = campaign.CampaignCode,
                                BillNo = campaign.BillNo
                            },
                            BillStatus = campaign.Status,
                            DueAmount = "0.000", // Open amount - donor chooses
                            IssueDate = campaign.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            DueDate = campaign.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            CloseDate = campaign.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            ServiceType = campaign.ServiceType,
                            BillType = campaign.BillType,
                            PmtConst = new PmtConst
                            {
                                AllowPart = campaign.AllowPartialPayment,
                                Lower = campaign.MinAmount.ToString("F3"),
                                Upper = campaign.MaxAmount.ToString("F3")
                            },
                            SubPmts = new List<SubPmt>
                            {
                                new SubPmt
                                {
                                    Amount = "0.000",
                                    SetBnkCode = campaign.BankCode,
                                    AcctNo = campaign.IBAN
                                }
                            },
                            AdditionalInfo = new AdditionalInfo
                            {
                                CustName = campaign.CustName,
                                FreeText = campaign.FreeText,
                                Email = campaign.Email,
                                Phone = campaign.Phone
                            },
                            BillCustomerCat = campaign.BillCustomerCat
                        }
                    }
                }
            }
        };

        _logger.LogInformation("BillPull success for campaign: {CampaignCode}", campaign.CampaignCode);
        return response;
    }

    public async Task<MfepPaymentNotificationResponse> HandlePaymentNotificationAsync(
        MfepPaymentNotificationRequest request)
    {
        var guid = request.MFEP.MsgHeader.GUID;
        var trxInf = request.MFEP.MsgBody.Transactions.TrxInf;
        var joebppsTrx = trxInf.JOEBPPSTrx;
        var billingNo = trxInf.AcctInfo.BillingNo;

        _logger.LogInformation(
            "PaymentNotification for JOEBPPSTrx: {JOEBPPSTrx}, BillingNo: {BillingNo}, GUID: {GUID}",
            joebppsTrx, billingNo, guid);

        // Check for duplicate transaction
        if (await _donationRepository.ExistsByJOEBPPSTrxAsync(joebppsTrx))
        {
            _logger.LogWarning("PaymentNotification: Duplicate JOEBPPSTrx: {JOEBPPSTrx}", joebppsTrx);
            return BuildPaymentNotificationErrorResponse(guid, joebppsTrx, trxInf.ProcessDate,
                trxInf.StmtDate, 4, "Duplicate transaction");
        }

        // Find campaign
        var campaign = await _campaignRepository.GetByCampaignCodeAsync(billingNo);
        if (campaign == null)
        {
            _logger.LogWarning("PaymentNotification: Campaign not found for BillingNo: {BillingNo}", billingNo);
            return BuildPaymentNotificationErrorResponse(guid, joebppsTrx, trxInf.ProcessDate,
                trxInf.StmtDate, 1, "Campaign not found");
        }

        // Parse amounts
        if (!decimal.TryParse(trxInf.PaidAmt, out var paidAmount))
        {
            _logger.LogWarning("PaymentNotification: Invalid PaidAmt: {PaidAmt}", trxInf.PaidAmt);
            return BuildPaymentNotificationErrorResponse(guid, joebppsTrx, trxInf.ProcessDate,
                trxInf.StmtDate, 3, "Invalid request format");
        }

        decimal.TryParse(trxInf.DueAmt, out var dueAmount);
        decimal.TryParse(trxInf.FeesAmt, out var feesAmount);

        DateTime.TryParse(trxInf.ProcessDate, out var processDate);
        DateTime.TryParse(trxInf.StmtDate, out var stmtDate);

        // Create donation record
        var donation = new Donation
        {
            CampaignId = campaign.Id,
            JOEBPPSTrx = joebppsTrx,
            BankTrxId = trxInf.BankTrxId,
            BankCode = trxInf.BankCode,
            BillingNo = billingNo,
            BillNo = trxInf.AcctInfo.BillNo,
            DueAmount = dueAmount,
            PaidAmount = paidAmount,
            FeesAmount = feesAmount,
            FeesOnBiller = trxInf.FeesOnBiller,
            PmtStatus = trxInf.PmtStatus,
            Currency = trxInf.Currency,
            AccessChannel = trxInf.AccessChannel,
            PaymentMethod = trxInf.PaymentMethod,
            PaymentType = trxInf.PaymentType,
            ProcessDate = processDate != default ? processDate : DateTime.UtcNow,
            StmtDate = stmtDate != default ? stmtDate : DateTime.UtcNow,
            PayerIdType = trxInf.PayerInfo?.IdType,
            PayerId = trxInf.PayerInfo?.Id,
            PayerNation = trxInf.PayerInfo?.Nation,
            RequestGUID = guid,
            CreatedAt = DateTime.UtcNow
        };

        await _donationRepository.CreateAsync(donation);

        // Update campaign totals using the repository (thread-safe via DB transaction)
        campaign.CollectedAmount += paidAmount;
        campaign.DonorsCount++;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _campaignRepository.UpdateAsync(campaign);

        _logger.LogInformation(
            "PaymentNotification processed. JOEBPPSTrx: {JOEBPPSTrx}, Amount: {Amount}, Campaign: {CampaignCode}",
            joebppsTrx, paidAmount, campaign.CampaignCode);

        return new MfepPaymentNotificationResponse
        {
            MFEP = new PaymentNotificationMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "BLRPMTNTFRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = 0,
                        ErrorDesc = "Success",
                        Severity = "Info"
                    }
                },
                MsgBody = new PaymentNotificationResponseBody
                {
                    Transactions = new PaymentNotificationResponseTransactions
                    {
                        TrxInf = new PaymentNotificationResponseTrxInf
                        {
                            JOEBPPSTrx = joebppsTrx,
                            ProcessDate = trxInf.ProcessDate,
                            STMTDate = trxInf.StmtDate,
                            Result = new MfepResult
                            {
                                ErrorCode = 0,
                                ErrorDesc = "Success",
                                Severity = "Info"
                            }
                        }
                    }
                }
            }
        };
    }

    public async Task<MfepPaymentAcknowledgmentResponse> HandlePaymentAcknowledgmentAsync(
        MfepPaymentAcknowledgmentRequest request)
    {
        var guid = request.MFEP.MsgHeader.GUID;
        var billingInfo = request.MFEP.MsgBody.BillingInfo;
        var joebppsTrx = billingInfo.JOEBPPSTrx;

        _logger.LogInformation("PaymentAcknowledgment for JOEBPPSTrx: {JOEBPPSTrx}, GUID: {GUID}",
            joebppsTrx, guid);

        // Find the donation
        var donation = await _donationRepository.GetByJOEBPPSTrxAsync(joebppsTrx);
        if (donation == null)
        {
            _logger.LogWarning("PaymentAcknowledgment: Donation not found for JOEBPPSTrx: {JOEBPPSTrx}",
                joebppsTrx);
            return BuildPaymentAcknowledgmentErrorResponse(guid, joebppsTrx,
                billingInfo.ProcessDate, billingInfo.StmtDate, 1, "Transaction not found");
        }

        // Update acknowledgment status
        donation.IsAcknowledged = true;
        donation.AcknowledgedAt = DateTime.UtcNow;
        donation.PmtStatus = billingInfo.PmtStatus;
        await _donationRepository.UpdateAsync(donation);

        _logger.LogInformation("PaymentAcknowledgment processed for JOEBPPSTrx: {JOEBPPSTrx}", joebppsTrx);

        return new MfepPaymentAcknowledgmentResponse
        {
            MFEP = new PaymentAcknowledgmentMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "PMTACKRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = 0,
                        ErrorDesc = "Success",
                        Severity = "Info"
                    }
                },
                MsgBody = new PaymentAcknowledgmentResponseBody
                {
                    Transactions = new PaymentAcknowledgmentResponseTransactions
                    {
                        TrxInf = new PaymentAcknowledgmentResponseTrxInf
                        {
                            JOEBPPSTrx = joebppsTrx,
                            ProcessDate = billingInfo.ProcessDate,
                            STMTDate = billingInfo.StmtDate,
                            Result = new MfepResult
                            {
                                ErrorCode = 0,
                                ErrorDesc = "Success",
                                Severity = "Info"
                            }
                        }
                    }
                }
            }
        };
    }

    private MfepBillPullResponse BuildBillPullErrorResponse(string guid, int errorCode, string errorDesc)
    {
        return new MfepBillPullResponse
        {
            MFEP = new BillPullMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "BILPULRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = errorCode,
                        ErrorDesc = errorDesc,
                        Severity = "Error"
                    }
                },
                MsgBody = new BillPullResponseBody
                {
                    RecCount = 0,
                    BillRec = new List<BillRecord>()
                }
            }
        };
    }

    private MfepPaymentNotificationResponse BuildPaymentNotificationErrorResponse(
        string guid, string joebppsTrx, string processDate, string stmtDate,
        int errorCode, string errorDesc)
    {
        return new MfepPaymentNotificationResponse
        {
            MFEP = new PaymentNotificationMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "BLRPMTNTFRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = errorCode,
                        ErrorDesc = errorDesc,
                        Severity = "Error"
                    }
                },
                MsgBody = new PaymentNotificationResponseBody
                {
                    Transactions = new PaymentNotificationResponseTransactions
                    {
                        TrxInf = new PaymentNotificationResponseTrxInf
                        {
                            JOEBPPSTrx = joebppsTrx,
                            ProcessDate = processDate,
                            STMTDate = stmtDate,
                            Result = new MfepResult
                            {
                                ErrorCode = errorCode,
                                ErrorDesc = errorDesc,
                                Severity = "Error"
                            }
                        }
                    }
                }
            }
        };
    }

    private MfepPaymentAcknowledgmentResponse BuildPaymentAcknowledgmentErrorResponse(
        string guid, string joebppsTrx, string processDate, string stmtDate,
        int errorCode, string errorDesc)
    {
        return new MfepPaymentAcknowledgmentResponse
        {
            MFEP = new PaymentAcknowledgmentMfepResponse
            {
                MsgHeader = new MsgHeader
                {
                    TmStp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GUID = guid,
                    TrsInf = new TrsInf
                    {
                        SdrCode = _billerCode,
                        ResTyp = "PMTACKRS"
                    },
                    Result = new MfepResult
                    {
                        ErrorCode = errorCode,
                        ErrorDesc = errorDesc,
                        Severity = "Error"
                    }
                },
                MsgBody = new PaymentAcknowledgmentResponseBody
                {
                    Transactions = new PaymentAcknowledgmentResponseTransactions
                    {
                        TrxInf = new PaymentAcknowledgmentResponseTrxInf
                        {
                            JOEBPPSTrx = joebppsTrx,
                            ProcessDate = processDate,
                            STMTDate = stmtDate,
                            Result = new MfepResult
                            {
                                ErrorCode = errorCode,
                                ErrorDesc = errorDesc,
                                Severity = "Error"
                            }
                        }
                    }
                }
            }
        };
    }
}
