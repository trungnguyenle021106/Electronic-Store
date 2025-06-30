using System.Security.Claims;

namespace UserService.Application.Service
{
    public class AuthService
    {
        public bool HaveAName(HttpContext httpContext)
        {
            var user = httpContext.User; // Lấy thông tin người dùng từ HttpContext
            var nameClaim = user.FindFirst(ClaimTypes.Name);
            string name = nameClaim?.Value ?? "";

            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            return false;
        }

        public bool IsLogOut(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }
    }
}
