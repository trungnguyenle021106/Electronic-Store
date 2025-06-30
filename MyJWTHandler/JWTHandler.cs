using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using MyJWTHandler.Domain;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyJWTHandler
{
    public static class JWTHandler
    {
        #region Token JWT
        private static string GenerateToken(LoginResponse result, WebApplication app)
        {
            int customerId = result.customer != null ? result.customer.ID : 0;
            string customerName = result.customer != null ? result.customer.Name : "";
            string customerPhone = result.customer != null ? result.customer.Phone : "";

            // Load các giá trị từ file .env
            DotNetEnv.Env.Load();

            // Đọc các giá trị từ file .env hoặc cấu hình của app
            var JwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? app.Configuration["Jwt:Issuer"] ?? "";
            var JwtAudiences = Environment.GetEnvironmentVariable("Jwt__Audience")?.Split(',')
                               ?? app.Configuration["Jwt:Audience"]?.Split(',')
                               ?? new string[] { }; // Lấy danh sách audience
            var JwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? app.Configuration["Jwt:Key"] ?? "";

            // Tạo token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtKey);

            // Tạo danh sách claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.idAccount.ToString() ?? "0"),
                new Claim(ClaimTypes.Role, result.role.ToString() ?? "false"),
                new Claim("CustomerName", customerName),
                new Claim("CustomerPhone", customerPhone),
                new Claim("CustomerId", customerId.ToString())
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Tạo token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion
    }
}