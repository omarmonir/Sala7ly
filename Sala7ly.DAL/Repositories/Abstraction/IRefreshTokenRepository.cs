using System;
using System.Collections.Generic;
using System.Text;
using Sala7ly.DAL.Entities;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateRefreshTokenAsync(string userId, string tokenString,int daysToExpire);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task RevokeAsync(RefreshToken token);
        Task<RefreshToken?> GetActiveByTokenAsync(string token);
        Task RevokeAllActiveAsync(string userId);

    }
}
