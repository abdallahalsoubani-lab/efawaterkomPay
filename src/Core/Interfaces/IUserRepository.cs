using DirectPayGateway.Core.DTOs.Admin;
using DirectPayGateway.Core.DTOs.Payment;
using DirectPayGateway.Core.Entities;

namespace DirectPayGateway.Core.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<PagedResult<ApplicationUser>> GetPagedAsync(UserListRequest filter);
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task UpdateAsync(ApplicationUser user);
}
