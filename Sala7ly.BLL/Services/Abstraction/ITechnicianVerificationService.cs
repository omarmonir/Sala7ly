using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.BLL.DTOs.VerificationDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface ITechnicianVerificationService
    {
        Task<VerificationDetailsDto?> GetByIdAsync(int id);
        Task<List<VerificationDetailsDto>> GetByTechnicianIdAsync(int technicianId);
        Task<List<VerificationDetailsDto>> GetPendingAsync();
        Task<bool> SubmitAsync(SubmitVerificationDto dto);
        Task<bool> ApproveAsync(int verificationId, string adminId);
        Task<bool> RejectAsync(RejectVerificationDto dto, string adminId);
    }
}