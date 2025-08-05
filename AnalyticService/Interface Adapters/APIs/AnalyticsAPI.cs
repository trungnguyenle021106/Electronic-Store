using CommonDto.ResultDTO;
using AnalyticService.Application.Usecases;
using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Request;
using Microsoft.AspNetCore.Mvc;
using AnalyticService.Domain.Response;

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
            app.MapPost("/analytics/order-by-date", async (HttpContext httpContext, AnalyzeOrderByDateUC analyzeOrderByDateUC, HandleResultApi handleResultApi,
                [FromBody] AnalyzeOrderRequest analyzeOrderRequest) =>
            {
                ServiceResult<OrderByDate> result = await analyzeOrderByDateUC.Analyze(analyzeOrderRequest);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void AnalyzeProductStatistics(this WebApplication app)
        {
            app.MapPost("/analytics/product-statistics", async (HttpContext httpContext, AnalyzeProductStatisticsUC analyzeProductStatisticsUC, HandleResultApi handleResultApi,
                [FromBody] List<ProductStatistics> listReq) =>
            {
                ServiceResult<ProductStatistics> result = await analyzeProductStatisticsUC.Analyze(listReq);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
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
            app.MapGet("/analytics/order-by-date", async (HttpContext httpContext, GetAnalyticsUC getAnalyticsUC, HandleResultApi handleResultApi,
                [FromQuery] DateTime a, [FromQuery] DateTime b) =>
            {
                ServiceResult<OrderByDate> result = await getAnalyticsUC.GetOrderByDateAnalyticsInTime(a,b);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        public static void GetProductStatisticsAnalytics(this WebApplication app)
        {
            app.MapGet("/analytics/product-statistics", async (HttpContext httpContext, GetAnalyticsUC GetAnalyticsUC, HandleResultApi handleResultApi,
                [FromQuery] int top) =>
            {
                ServiceResult<ProductStatisticResponse> result = await GetAnalyticsUC.GetProductStatisticsAnalytics(top);

                return handleResultApi.MapServiceResultToHttp(result);
            }).RequireAuthorization("OnlyAdmin");
        }

        #endregion
    }
}
