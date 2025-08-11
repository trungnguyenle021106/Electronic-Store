using AnalyticService.Application.Usecases;
using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Request;
using AnalyticService.Domain.Response;
using Azure;
using CommonDto.ResultDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace AnalyticService.Interface_Adapters.APIs
{
    public static class AnalyticsAPI
    {
        public static void MapAnalyzeEndpoints(this WebApplication app)
        {
            MapAnalyzeUseCaseAPIs(app);
            MapGetAnalyticsUseCaseAPIs(app);
        }

        #region Analyze USECASE
        public static void MapAnalyzeUseCaseAPIs(this WebApplication app)
        {
            AnalyzeOrderByDate(app);
            AnalyzeProductStatistics(app);
        }
        public static void AnalyzeOrderByDate(this WebApplication app)
        {
            app.MapPost("/analytics/order-by-date",
                async (HttpContext httpContext, AnalyzeOrderByDateUC analyzeOrderByDateUC, HandleResultApi handleResultApi,
                    [FromBody] AnalyzeOrderRequest analyzeOrderRequest) =>
                {
                    ServiceResult<OrderByDate> result = await analyzeOrderByDateUC.Analyze(analyzeOrderRequest);
                    return handleResultApi.MapServiceResultToHttp(result);
                })
                .RequireAuthorization("OnlyAdmin")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Phân tích đơn hàng theo ngày (POST)";
                    operation.Description = "Gửi yêu cầu phân tích dữ liệu đơn hàng trong một khoảng thời gian cụ thể.";
                    operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Analytics" } };
                    return operation;
                });
        }

        public static void AnalyzeProductStatistics(this WebApplication app)
        {
            app.MapPost("/analytics/product-statistics", async (HttpContext httpContext, AnalyzeProductStatisticsUC analyzeProductStatisticsUC, HandleResultApi handleResultApi,
                    [FromBody] List<ProductStatistics> listReq) =>
            {
                ServiceResult<ProductStatistics> result = await analyzeProductStatisticsUC.Analyze(listReq);
                return handleResultApi.MapServiceResultToHttp(result);
            })
                .RequireAuthorization("OnlyAdmin")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Phân tích thống kê sản phẩm (POST)";
                    operation.Description = "Gửi yêu cầu phân tích thống kê cho các sản phẩm đã cho.";
                    operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Analytics" } };
                    return operation;
                });
        }

        #endregion


        #region Get Analytics USECASE
        public static void MapGetAnalyticsUseCaseAPIs(this WebApplication app)
        {
            GetOrderByDateAnalytics(app);
            GetProductStatisticsAnalytics(app);
        }

        public static void GetOrderByDateAnalytics(this WebApplication app)
        {
            app.MapGet("/analytics/order-by-date",
                async (HttpContext httpContext, GetAnalyticsUC getAnalyticsUC, HandleResultApi handleResultApi,
                    [FromQuery] DateTime a, [FromQuery] DateTime b) =>
                {
                    ServiceResult<OrderByDate> result = await getAnalyticsUC.GetOrderByDateAnalyticsInTime(a, b);
                    return handleResultApi.MapServiceResultToHttp(result);
                })
                .RequireAuthorization("OnlyAdmin")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy dữ liệu phân tích đơn hàng theo ngày (GET)";
                    operation.Description = "Truy vấn dữ liệu thống kê đơn hàng trong một khoảng thời gian.";
                    operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Analytics" } };

                    // Mô tả các tham số query
                    operation.Parameters[0].Description = "Ngày bắt đầu phân tích (định dạng yyyy-MM-dd)";
                    operation.Parameters[1].Description = "Ngày kết thúc phân tích (định dạng yyyy-MM-dd)";
                    return operation;
                });
        }

        public static void GetProductStatisticsAnalytics(this WebApplication app)
        {
            app.MapGet("/analytics/product-statistics", async (HttpContext httpContext, GetAnalyticsUC GetAnalyticsUC, HandleResultApi handleResultApi,
                    [FromQuery] int top) =>
            {
                ServiceResult<ProductStatisticResponse> result = await GetAnalyticsUC.GetProductStatisticsAnalytics(top);
                return handleResultApi.MapServiceResultToHttp(result);
            })
                .RequireAuthorization("OnlyAdmin")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy dữ liệu thống kê sản phẩm (GET)";
                    operation.Description = "Truy vấn dữ liệu thống kê sản phẩm theo số lượng sản phẩm hàng đầu.";
                    operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Analytics" } };

                    // Mô tả tham số query
                    operation.Parameters[0].Description = "Số lượng sản phẩm hàng đầu muốn lấy (ví dụ: 10, 20).";
                    return operation;
                });
        }

        #endregion
    }
}
