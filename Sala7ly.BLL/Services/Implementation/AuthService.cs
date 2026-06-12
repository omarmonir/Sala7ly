using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sala7ly.BLL.DTOs.Auth;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;


namespace Sala7ly.BLL.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _signInManager;
        private readonly int _refreshTokenValidityInDays;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRefreshTokenRepository _refreshTokenRepository;   
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailService emailService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _refreshTokenValidityInDays =
             configuration.GetValue<int>("Jwt:RefreshTokenValidityInDays");
            _refreshTokenRepository = refreshTokenRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }


        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)??
                await _userManager.FindByNameAsync(email);

            if (user == null)
                throw new KeyNotFoundException("البريد الإلكتروني غير مسجل في النظام");

            var otp = GenerateOtp();
            var otpDigits = otp.PadLeft(6, '0');

            user.SetOtp(otp, DateTime.UtcNow.AddMinutes(10));
            await _userManager.UpdateAsync(user);

            var emailBody = BuildOtpEmail(user.UserName!, user.Email!, otpDigits);
            var sent = await _emailService.SendEmailAsync(email, "إعادة تعيين كلمة المرور - تكامل", emailBody);

            if (!sent)
                throw new Exception("فشل إرسال البريد الإلكتروني، يرجى المحاولة لاحقاً");

            return "تم إرسال رمز التحقق إلى بريدك الإلكتروني";
        }
        public async Task<bool> RegisterAdminAsync(AdminRegisterDto dto)
        {
           
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                throw new InvalidOperationException("البريد الإلكتروني مستخدم بالفعل");

            var user = new User
            {
                Name = dto.Name,
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // ensure role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            await _userManager.AddToRoleAsync(user, "Admin");
            return true;
        }
        public async Task<ResponseLoginDto> LoginAsync(RequestLoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email) ??
                await _userManager.FindByNameAsync(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("بيانات الاعتماد غير صحيحة");

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnauthorizedAccessException("كلمة المرور غير صحيحة");

            if (!user.IsActive || user.LockoutEnd > DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("الحساب معطل حالياً. يرجى التواصل مع الإدارة");

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = await CreateJwtToken(user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = await _refreshTokenRepository.CreateRefreshTokenAsync(user.Id, GenerateRefreshTokenString(), _refreshTokenValidityInDays);

            await _signInManager.SignInAsync(user, isPersistent: true);

            user.RecordLogin();
            await _userManager.UpdateAsync(user);

            return new ResponseLoginDto
            {
                Token = accessToken,
                Expiration = jwtToken.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires,
                Roles = roles.ToList()
            };
        }


        public async Task<string> LogoutByTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken);

            if (token != null)
                await _refreshTokenRepository.RevokeAsync(token);

            await _signInManager.SignOutAsync();
            return "تم تسجيل الخروج بنجاح";
        }

        public async Task<string> DeactivateAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("المستخدم غير موجود");

            user.Deactivate();
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _refreshTokenRepository.RevokeAllActiveAsync(user.Id);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return "تم تعطيل الحساب بنجاح";
        }

        public async Task<string> ReactivateAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("المستخدم غير موجود");

            user.Activate();
            user.LockoutEnabled = false;
            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return "تم إعادة تفعيل الحساب بنجاح";
        }

        public async Task<ResponseLoginDto> RefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null || refreshToken.IsRevoked)
                throw new UnauthorizedAccessException("انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً");

            if (refreshToken.IsExpired)
                throw new UnauthorizedAccessException("انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً");

            var user = refreshToken.User;

            if (!user.IsActive || user.LockoutEnd > DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("الحساب معطل حالياً");
            var jwtToken = CreateJwtToken(user).Result;
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var roles = await _userManager.GetRolesAsync(user);

            return new ResponseLoginDto
            {
                Token = accessToken,
                Expiration = jwtToken.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.Expires,
                Roles = roles.ToList()
            };
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return "المستخدم غير موجود";
            if (user.ResetOtp != null)
                throw new UnauthorizedAccessException(
                    "يجب التحقق من رمز OTP أولاً قبل إعادة تعيين كلمة المرور");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new InvalidOperationException("كلمة المرور غير متطابقة");


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            return result.Succeeded
                ? "تم تغيير كلمة المرور بنجاح"
                : string.Join(", ", result.Errors.Select(e => e.Description));
        }

        public async Task<string> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return "بيانات غير صحيحة";
            if (user.FailedOtpAttempts >= 5) return "تم تجاوز عدد المحاولات المسموح";
            if (user.ResetOtpExpiry < DateTime.UtcNow) return "انتهت صلاحية الكود";

            if (user.ResetOtp != otp)
            {
                user.IncrementFailedOtpAttempts();
                await _userManager.UpdateAsync(user);
                return "الكود غير صحيح";
            }

            user.ClearOtp();
            await _userManager.UpdateAsync(user);
            return "OTP_VALID";
        }
        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
             
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(
                                        _configuration.GetValue<int>("Jwt:TokenValidityInMinutes")),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private static string GenerateOtp()
        {
            return new Random().Next(0, 999999).ToString();
        }
        private static string BuildOtpEmail(
    string userName, string userEmail, string otpDigits)
        {
            return $@"<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>إعادة تعيين كلمة المرور - صلّحلي</title>
</head>
<body style='margin:0;padding:0;background:#f4f6f8;font-family:Cairo,Arial,sans-serif;direction:rtl;'>

<table border='0' cellpadding='0' cellspacing='0' width='100%' style='background:#f4f6f8;padding:40px 16px;'>
  <tr>
    <td align='center'>
      <table border='0' cellpadding='0' cellspacing='0' width='520'
             style='max-width:520px;width:100%;background:#ffffff;border-radius:16px;
                    border:1px solid #e2e8f0;overflow:hidden;'>

        <tr>
          <td align='center'
              style='background:#1a2744;padding:28px 32px 22px;border-bottom:4px solid #f97316;'>
            <div style='font-size:26px;font-weight:900;color:#ffffff;letter-spacing:-0.5px;'>
              &#128296; صلّحلي
            </div>
            <div style='font-size:11px;color:#94a3b8;margin-top:6px;letter-spacing:1px;'>
              منصة الصيانة المنزلية الذكية
            </div>
            <div style='display:inline-block;background:rgba(249,115,22,0.15);
                        border:1px solid rgba(249,115,22,0.4);border-radius:20px;
                        padding:4px 14px;font-size:11px;color:#fb923c;margin-top:12px;'>
              إعادة تعيين كلمة المرور
            </div>
          </td>
        </tr>

        <tr>
          <td style='padding:28px 32px;'>
            <p style='font-family:Cairo,Arial,sans-serif;font-size:15px;color:#1a2744;margin:0 0 6px 0;'>
              مرحباً <strong style='color:#0f172a;'>{userName}</strong>،
            </p>
            <p style='font-family:Cairo,Arial,sans-serif;font-size:13px;color:#64748b;
                      line-height:1.9;margin:0 0 24px 0;'>
              تلقّينا طلباً لإعادة تعيين كلمة مرور حسابك في منصة صلّحلي.<br/>
              استخدم الرمز أدناه لإتمام العملية — الرمز شخصي ولا تشاركه مع أحد.
            </p>

            <!-- OTP Box -->
            <table border='0' cellpadding='0' cellspacing='0' width='100%'
                   style='background:#fff8f3;border-radius:14px;border:1px solid #fed7aa;
                          margin-bottom:20px;'>
              <tr>
                <td align='center' style='padding:24px 20px;'>
                  <p style='font-family:Cairo,Arial,sans-serif;font-size:11px;color:#ea580c;
                            font-weight:700;letter-spacing:1px;margin:0 0 14px 0;'>رمز التحقق</p>
                  <table border='0' cellpadding='0' cellspacing='0'
                         style='margin:0 auto 14px auto;background:#ffffff;
                                border:2px solid #f97316;border-radius:10px;'>
                    <tr>
                      <td align='center'
                          style='padding:14px 32px;font-family:Cairo,Arial,monospace;
                                 font-size:30px;font-weight:900;color:#1a2744;letter-spacing:14px;'>
                        {otpDigits}
                      </td>
                    </tr>
                  </table>
                  <div style='display:inline-block;background:#fef9c3;border:1px solid #fde047;
                              border-radius:20px;padding:4px 14px;
                              font-family:Cairo,Arial,sans-serif;font-size:11.5px;color:#92400e;'>
                    &#9201; صالح لمدة <strong>10 دقائق</strong> فقط
                  </div>
                </td>
              </tr>
            </table>

            <table border='0' cellpadding='0' cellspacing='0' width='100%'
                   style='border:1px solid #e2e8f0;border-radius:10px;margin-bottom:16px;
                          border-collapse:collapse;overflow:hidden;'>
              <tr>
                <td style='padding:10px 14px;border-bottom:1px solid #e2e8f0;
                           font-family:Cairo,Arial,sans-serif;font-size:12px;color:#94a3b8;'>
                  البريد الإلكتروني
                </td>
                <td align='left'
                    style='padding:10px 14px;border-bottom:1px solid #e2e8f0;
                           font-family:Cairo,Arial,sans-serif;font-size:12px;color:#1a2744;'>
                  {userEmail}
                </td>
              </tr>
              <tr>
                <td style='padding:10px 14px;font-family:Cairo,Arial,sans-serif;
                           font-size:12px;color:#94a3b8;'>
                  وقت الطلب
                </td>
                <td align='left'
                    style='padding:10px 14px;font-family:Cairo,Arial,sans-serif;
                           font-size:12px;color:#1a2744;'>
                  {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
                </td>
              </tr>
            </table>

            <table border='0' cellpadding='0' cellspacing='0' width='100%'
                   style='background:#fffbeb;border:1px solid #fde68a;border-radius:10px;'>
              <tr>
                <td style='padding:12px 14px;'>
                  <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                    <tr>
                      <td width='24' valign='top'
                          style='font-size:15px;padding-left:10px;'>&#9888;&#65039;</td>
                      <td style='font-family:Cairo,Arial,sans-serif;font-size:12px;
                                 color:#92400e;line-height:1.7;'>
                        إذا لم تكن أنت من طلب إعادة تعيين كلمة المرور، يُرجى تجاهل هذه الرسالة.
                        حسابك آمن ولن يتم إجراء أي تغيير.
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <tr>
          <td align='center'
              style='background:#f8fafc;padding:16px;border-top:1px solid #e2e8f0;
                     font-family:Cairo,Arial,sans-serif;font-size:11px;color:#94a3b8;'>
            جميع الحقوق محفوظة &copy; 2026 &nbsp;|&nbsp;
            <span style='color:#f97316;font-weight:700;'>صلّحلي</span>
            &nbsp;|&nbsp; صُنع في مصر &#127466;&#127468;
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>

</body>
</html>";
        }
    }
    
}
