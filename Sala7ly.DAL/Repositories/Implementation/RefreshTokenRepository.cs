using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.DAL.Repositories.Implementation
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<RefreshToken> CreateRefreshTokenAsync(string userId,
            string tokenString,
            int daysToExpire)
        {
            var refreshToken = RefreshToken.Create(userId, tokenString, daysToExpire);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }
        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(rt => rt.Token == token);
        }
        public async Task<RefreshToken?> GetActiveByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token && r.Revoked == null);
        }
        public async Task RevokeAsync(RefreshToken token)
        {
            token.Revoke();
            await _context.SaveChangesAsync();
        }
        public async Task RevokeAllActiveAsync(string userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.Revoked == null)
                .ToListAsync();

            foreach (var t in activeTokens)
                t.Revoke();

            await _context.SaveChangesAsync();
        }
    }
}
