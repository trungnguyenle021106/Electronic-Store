using AnalyticService.Domain.Entities;
using AnalyticService.Domain.Interface.UnitOfWork;
using AnalyticService.Domain.Request;
using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;

namespace AnalyticService.Application.Usecases
{
    public class AnalyzeOrderByDateUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly HandleServiceError handleServiceError;

        public AnalyzeOrderByDateUC(IUnitOfWork unitOfWork, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<OrderByDate>> Analyze(AnalyzeOrderRequest analyzeRequest)
        {
            if (analyzeRequest == null)
            {
                return ServiceResult<OrderByDate>.Failure(
                    "AnalyzeOrderRequest data cannot be null.",
                    ServiceErrorType.ValidationError);
            }
            try
            {
                IQueryable<OrderByDate> query = this.unitOfWork.OrderByDateRepository().GetAll();
                OrderByDate? orderByDate = await query.Where(item => item.Date == analyzeRequest.Date).FirstOrDefaultAsync();

                if (orderByDate == null)
                {
                    orderByDate = await this.unitOfWork.OrderByDateRepository().Add(new OrderByDate
                    {
                        Date = analyzeRequest.Date,
                        TotalOrders = 1,
                        TotalRevenue = (decimal)analyzeRequest.Total,
                        CancelledOrders = analyzeRequest.CancelledOrders
                    });

                    await this.unitOfWork.Commit();
                    return ServiceResult<OrderByDate>.Success(orderByDate);
                }

                orderByDate.TotalOrders += 1;
                orderByDate.TotalRevenue += (decimal)analyzeRequest.Total;
                orderByDate.CancelledOrders += analyzeRequest.CancelledOrders;

                await this.unitOfWork.Commit();
                return ServiceResult<OrderByDate>.Success(orderByDate);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Analyze Order By Date: {ex}");

                return ServiceResult<OrderByDate>.Failure(
                    "An unexpected error occurred while Analyze Order By Date. Please try again.", ServiceErrorType.InternalError);
            }
        }
    }
}
