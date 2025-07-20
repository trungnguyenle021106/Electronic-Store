using System.Net.Http.Headers;

namespace ContentManagementService.Infrastructure.Handler
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthHeaderHandler> _logger;

        // Inject IHttpContextAccessor để truy cập HttpContext
        // Inject ILogger để ghi log các vấn đề liên quan đến token
        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor, ILogger<AuthHeaderHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Chỉ thêm header xác thực nếu có HttpContext và người dùng có thể được xác thực
            if (httpContext != null)
            {
                string? accessToken = httpContext.Request.Cookies["AccessToken"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Xử lý tiền tố "Bearer " nếu token có nó (JWT từ cookie thường không có)
                    if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        accessToken = accessToken.Substring("Bearer ".Length).Trim();
                    }
                    // Thêm Authorization Header vào HttpRequestMessage hiện tại
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                else
                {
                    // Ghi log cảnh báo nếu không tìm thấy token nhưng yêu cầu có thể cần nó
                    _logger.LogWarning($"Access Token not found in cookie for current HTTP context. Request to {request.RequestUri?.AbsolutePath} might fail due to unauthorized access at downstream service.");
                }
            }
            else
            {
                // Trường hợp HttpContext là null (ví dụ: background services)
                // Trong API Gateway, điều này ít xảy ra cho các request từ frontend
                _logger.LogWarning("HttpContext is null in AuthHeaderHandler. Cannot set Authorization header.");
            }

            // Chuyển tiếp yêu cầu đến handler tiếp theo trong pipeline
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
