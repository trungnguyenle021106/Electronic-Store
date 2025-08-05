using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Interface.UnitOfWork;
using AnalyticService.Domain.Response;
using AnalyticService.Infrastructure.DTO;
using AnalyticService.Infrastructure.Service;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;

namespace AnalyticService.Application.Usecases
{
    public class GetAnalyticsUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;

        public GetAnalyticsUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<OrderByDate>> GetOrderByDateAnalyticsInTime(DateTime a, DateTime b)
        {
            try
            {
                IQueryable<OrderByDate> query = this.unitOfWork.OrderByDateRepository().GetAll();
                List<OrderByDate> orderByDates = await query.Where(item => item.Date >= a && item.Date <= b).ToListAsync() ?? new List<OrderByDate>();

                return ServiceResult<OrderByDate>.Success(orderByDates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GetOrderByDateAnalytics {ex}");

                return ServiceResult<OrderByDate>.Failure(
                    "An unexpected error occurred while GetOrderByDateAnalytics. Please try again.", ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<ProductStatisticResponse>> GetProductStatisticsAnalytics(int top)
        {
            try
            {
                IQueryable<ProductStatistics> query = this.unitOfWork.ProductStatisticsRepository().GetAll();
                List<ProductStatistics> productStatistics = await query
                    .OrderBy(item => item.TotalSales)
                    .Take(top)
                    .ToListAsync() ?? new List<ProductStatistics>();

                if(productStatistics == null || !productStatistics.Any())
                {
                    return ServiceResult<ProductStatisticResponse>.Success(new List<ProductStatisticResponse>());
                }

                ServiceResult<Product> result = await this.productService.CheckExistProducts(productStatistics.Select(item => item.ProductID).ToList());
                if (!result.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                        MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "Check Product");
                    return ServiceResult<ProductStatisticResponse>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<ProductStatisticResponse> list = result.ListItem.Select(item => new ProductStatisticResponse
                {
                    Product = item,
                    TotalSales = productStatistics.FirstOrDefault(ps => ps.ProductID == item.ID)?.TotalSales ?? 0
                }).ToList();

                return ServiceResult<ProductStatisticResponse>.Success(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GetProductStatisticsAnalytics {ex}");
                return ServiceResult<ProductStatisticResponse>.Failure(
                    "An unexpected error occurred while GetProductStatisticsAnalytics. Please try again.", ServiceErrorType.InternalError);
            }
        }
    }
}
