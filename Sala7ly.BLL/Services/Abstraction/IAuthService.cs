using System;
using System.Collections.Generic;
using System.Text;
using Sala7ly.BLL.DTOs.Auth;

namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IAuthService
    {
        Task<ResponseLoginDto> LoginAsync(RequestLoginDto request);
        Task<bool> RegisterAdminAsync(AdminRegisterDto dto);
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDto dto);
        Task<string> LogoutByTokenAsync(string refreshToken);
        Task<string> DeactivateAccountAsync(string email);
        Task<string> ReactivateAccountAsync(string email);
        Task<string> VerifyOtpAsync(string email, string otp);
        Task<ResponseLoginDto> RefreshTokenAsync(string token);
    }
}
