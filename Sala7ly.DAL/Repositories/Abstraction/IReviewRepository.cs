using System.Collections.Generic;
using System.Threading.Tasks;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task<Review?> GetByRequestIdAsync(int requestId);
        Task<List<Review>> GetByRevieweeIdAsync(string revieweeId);
        Task<List<Review>> GetByReviewerIdAsync(string reviewerId);
        Task<bool> HasReviewForRequestAsync(int requestId, string reviewerId);
        Task AddAsync(Review review);
        void Update(Review review);
        void Delete(Review review);
        Task<int> SaveChangesAsync();
    }
}