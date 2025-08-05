using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Interface.UnitOfWork;
using AnalyticService.Domain.Request;
using AnalyticService.Infrastructure.DTO;
using AnalyticService.Infrastructure.Service;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;

namespace AnalyticService.Application.Usecases
{
    public class AnalyzeProductStatisticsUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;

        public AnalyzeProductStatisticsUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<ProductStatistics>> Analyze(List<ProductStatistics> listRequest)
        {
            if (listRequest == null)
            {
                return ServiceResult<ProductStatistics>.Failure(
                    "ProductStatistics data cannot be null.",
                    ServiceErrorType.ValidationError);
            }

            try
            {
                ServiceResult<Product> result = await this.productService.CheckExistProducts(listRequest.Select(item => item.ProductID).ToList());
                if (!result.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                        MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "Check Product");
                    return ServiceResult<ProductStatistics>.Failure(errorResult.Message, errorResult.ServiceErrorType);
                }

                List<ProductStatistics> listExist = new List<ProductStatistics>();
                foreach (ProductStatistics productStatistics in listRequest)
                {
                    if (result.ListItem.Select(item => item.ID).Contains(productStatistics.ProductID))
                    {
                        listExist.Add(productStatistics);
                    }
                }

                if (!listExist.Any())
                {
                    return ServiceResult<ProductStatistics>.Failure(
                        "No valid products found in the request.",
                        ServiceErrorType.ValidationError);
                }

                // --- BẮT ĐẦU SỬA LỖI TẠI ĐÂY ---

                // Tạo một danh sách các ProductID duy nhất từ listExist
                List<int> productIdsToAnalyze = listExist.Select(ps => ps.ProductID).ToList();

                // Thực hiện một truy vấn duy nhất để lấy tất cả các bản ghi ProductStatistics cần tìm
                IQueryable<ProductStatistics> query = this.unitOfWork.ProductStatisticsRepository().GetAll();

                ProductStatistics[] existingProducts = await query
                    .Where(item => productIdsToAnalyze.Contains(item.ProductID))
                    .ToArrayAsync(); // Sử dụng ToArrayAsync() để thực thi truy vấn

                // --- KẾT THÚC SỬA LỖI ---

                Dictionary<int, ProductStatistics> existingProductsMap = new Dictionary<int, ProductStatistics>();
                foreach (ProductStatistics p in existingProducts.Where(p => p != null))
                {
                    existingProductsMap[p.ProductID] = p;
                }

                List<ProductStatistics> productsToAdd = new List<ProductStatistics>();

                foreach (ProductStatistics requestProduct in listRequest)
                {
                    if (existingProductsMap.TryGetValue(requestProduct.ProductID, out ProductStatistics existingProduct))
                    {
                        existingProduct.TotalSales += requestProduct.TotalSales;
                    }
                    else
                    {
                        productsToAdd.Add(requestProduct);
                    }
                }

                await this.unitOfWork.ProductStatisticsRepository().AddRangeAsync(productsToAdd);
                await this.unitOfWork.Commit();

                return ServiceResult<ProductStatistics>.Success(existingProducts.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating filter and details: {ex}");

                return ServiceResult<ProductStatistics>.Failure(
                    "An unexpected error occurred while Analyze Product Statistics. Please try again.", ServiceErrorType.InternalError);
            }
        }
    }
}
