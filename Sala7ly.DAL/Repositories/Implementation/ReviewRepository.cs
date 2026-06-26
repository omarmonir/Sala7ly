using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Review?> GetByIdAsync(int id)
            => _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);

        public Task<Review?> GetByRequestIdAsync(int requestId)
            => _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.IsDeleted != true);

        public Task<List<Review>> GetByRevieweeIdAsync(string revieweeId)
            => _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeID == revieweeId && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public Task<List<Review>> GetByReviewerIdAsync(string reviewerId)
            => _context.Reviews
                .Include(r => r.Reviewee)
                .Where(r => r.ReviewerID == reviewerId && r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public Task<bool> HasReviewForRequestAsync(int requestId, string reviewerId)
            => _context.Reviews
                .AnyAsync(r => r.RequestId == requestId
                            && r.ReviewerID == reviewerId
                            && r.IsDeleted != true);

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public void Update(Review review)
        {
            _context.Reviews.Update(review);
        }

        public void Delete(Review review)
        {
            review.ToggaleStatus(review.DeletedBy ?? "system");
            _context.Reviews.Update(review);
        }

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}