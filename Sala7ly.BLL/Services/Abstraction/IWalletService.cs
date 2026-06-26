using Sala7ly.BLL.DTOs.WalletDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletAsync(string userId, int page, int pageSize);
        Task TopUpAsync(string userId, TopUpDto dto);
        Task WithdrawAsync(string userId, WithdrawDto dto);
        Task CreditAsync(string userId, decimal amount, string description, int? escrowId = null);
        Task DebitAsync(string userId, decimal amount, string description, int? escrowId = null);
    }
}
