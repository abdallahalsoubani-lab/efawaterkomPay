using DirectPayGateway.Core.DTOs.Admin;

namespace DirectPayGateway.Core.Interfaces;

public interface IStatisticsService
{
    Task<DashboardStatistics> GetDashboardStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
