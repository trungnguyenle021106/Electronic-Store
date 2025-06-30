using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Domain.DTO;
using UserService.Domain.Entities;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.Setting;

namespace UserService.Application.Service
{
    public class TokenService
    {
        public readonly JWTSetting _jwtSetting;
        private readonly IUnitOfWork unitOfWork;

        public TokenService(JWTSetting jwtSetting, IUnitOfWork unitOfWork)
        {
            this._jwtSetting = jwtSetting;
            this.unitOfWork = unitOfWork;
        }

        public string GenerateAccessToken(JWTClaim jWTClaim)
        {

            var JwtIssuer = _jwtSetting.Issuer;
            var JwtAudiences = _jwtSetting.Audiences;
            var JwtKey = _jwtSetting.Key;
            int ExpirationMinutes = _jwtSetting.ExpirationMinutes;

            // Tạo token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtKey);

            // Tạo danh sách claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, jWTClaim.IDAccount.ToString()),
                new Claim(ClaimTypes.Role, jWTClaim.Role.ToString()),
            };

            // Thêm các audience vào claims
            foreach (var audience in JwtAudiences)
            {
                claims.Add(new Claim("aud", audience));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = JwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            };

            // Tạo token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public void SetTokenCookie(HttpContext httpContext, string cookieName, string tokenValue,
            DateTimeOffset expiryDate, bool isHttpOnly = true, bool isSecure = false,
            SameSiteMode sameSite = SameSiteMode.Lax)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext), "HttpContext không được null.");
            }
            if (string.IsNullOrEmpty(cookieName))
            {
                throw new ArgumentException("Tên cookie không được rỗng.", nameof(cookieName));
            }
            if (string.IsNullOrEmpty(tokenValue))
            {
                throw new ArgumentException("Giá trị token không được rỗng.", nameof(tokenValue));
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = isHttpOnly,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = expiryDate
            };

            httpContext.Response.Cookies.Append(cookieName, tokenValue, cookieOptions);
        }

        public async Task<RefreshToken?> GetRefreshToken(HttpContext httpContext)
        {
            if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out string refreshTokenValue) || string.IsNullOrEmpty(refreshTokenValue))
            {
                return null;
            }

            RefreshToken? refreshToken = await this.unitOfWork.RefreshTokenRepository()
                .GetAll()
                .AsNoTracking()
                .Where(token => token.TokenHash == refreshTokenValue)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return refreshToken; // Trả về refreshToken (có thể là null)
        }

        public JWTClaim? GetJWTClaim(HttpContext httpContext)
        {
            var user = httpContext.User;

            // Kiểm tra xem người dùng đã được xác thực chưa
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null; // Người dùng chưa xác thực
            }

            int idAccount = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var Role = user.FindFirst(ClaimTypes.Role)?.Value;

            return new JWTClaim(idAccount, Role);
        }

        public void ClearTokenCookie(HttpContext context, string cookieName, bool secure = false, SameSiteMode sameSite = SameSiteMode.Lax)
        {
            context.Response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                IsEssential = true
            });
        }
    }
}
