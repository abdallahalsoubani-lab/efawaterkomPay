using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetByIdAsync(int id);
    Task<PaymentTransaction?> GetByBillerTrxNoAsync(string billerTrxNo);
    Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction);
    Task<PagedResult<PaymentTransaction>> GetPagedAsync(TransactionFilterRequest filter);
    Task<List<PaymentTransaction>> GetByUserIdAsync(string userId, int? limit = null);
    Task AddLogAsync(TransactionLog log);
    Task<bool> ExistsByBillerTrxNoAsync(string billerTrxNo);
}
