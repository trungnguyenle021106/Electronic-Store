using CommonDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderService.Domain.Interface.UnitOfWork;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OrderService.Application.Service
{
    public static class TokenService
    {
        public static JWTClaim? GetJWTClaim(HttpContext httpContext)
        {
            var user = httpContext.User;

            // Kiểm tra xem người dùng đã được xác thực chưa
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null; // Người dùng chưa xác thực
            }

            int idAccount = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var Role = user.FindFirst(ClaimTypes.Role)?.Value;

            int? customerId = null; // Khởi tạo là null
            if (Role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                var customerIdClaim = user.FindFirst("CustomerID");
                if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int parsedCustomerId))
                {
                    customerId = parsedCustomerId; // Gán giá trị nếu tìm thấy và parse thành công
                }
            }
            return new JWTClaim(idAccount, Role, customerId);
        }
    }
}
