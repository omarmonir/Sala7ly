using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sala7ly.BLL.DTOs.Auth;
using Sala7ly.BLL.Services.Abstraction;


namespace Sala7ly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        
        public AuthController(IAuthService auth) => _auth = auth;

   
        /// <summary>Login with email/username and password</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RequestLoginDto dto)
        {
            try
            {
                var result = await _auth.LoginAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Get new access token using refresh token</summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            try
            {
                var result = await _auth.RefreshTokenAsync(token);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Logout and revoke refresh token</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                var result = await _auth.LogoutByTokenAsync(refreshToken);
                return Ok(new { message = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Send OTP to email for password reset</summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                var result = await _auth.ForgotPasswordAsync(email);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Verify OTP code</summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var result = await _auth.VerifyOtpAsync(request.Email, request.Otp);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Reset password after OTP is verified</summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var result = await _auth.ResetPasswordAsync(dto);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Deactivate an account</summary>
        [HttpPatch("deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate([FromBody] string email)
        {
            try
            {
                var result = await _auth.DeactivateAccountAsync(email);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }

        /// <summary>Reactivate a deactivated account</summary>
        [HttpPatch("reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate([FromBody] string email)
        {
            try
            {
                var result = await _auth.ReactivateAccountAsync(email);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ غير متوقع", details = ex.Message });
            }
        }
    }

    public record VerifyOtpRequest(string Email, string Otp);
}
