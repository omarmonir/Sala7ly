namespace Sala7ly.BLL.DTOs.Auth
{
    public class ResponseLoginDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; init; }
        public DateTime RefreshTokenExpiration { get; init; }
        public List<string> Roles { get; init; }
    }
}
