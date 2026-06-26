using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.BLL.DTOs.ReviewDTOs;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IReviewService
    {
        Task<ReviewResponseDto?> GetByIdAsync(int id);
        Task<ReviewResponseDto?> GetByRequestIdAsync(int requestId);
        Task<List<ReviewResponseDto>> GetForTechnicianAsync(string technicianUserId);
        Task<bool> CreateAsync(string reviewerUserId, CreateReviewDto dto);
        Task<List<ReviewResponseDto>> GetAllAsync();
        Task<bool> ModerateAsync(string adminId, ModerateReviewDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddTechnicianReplyAsync(string technicianUserId, TechnicianReplyDto dto);
    }
}