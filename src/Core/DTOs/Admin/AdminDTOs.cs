using DirectPayGateway.Core.DTOs.Auth;

namespace DirectPayGateway.Core.DTOs.Admin;

public class DashboardStatistics
{
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public List<DailyStatistic> DailyStatistics { get; set; } = new();
    public List<StatusBreakdown> StatusBreakdown { get; set; } = new();
}

public class DailyStatistic
{
    public DateTime Date { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}

public class StatusBreakdown
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class UserListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class UserDetailDto : UserDto
{
    public int TransactionCount { get; set; }
    public decimal TotalTransactionAmount { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
}

public class ExportRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public string Format { get; set; } = "csv";
}
