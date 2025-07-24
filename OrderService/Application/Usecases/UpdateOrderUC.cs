using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.UnitOfWork;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.Data.DBContext;

namespace OrderService.Application.Usecases
{
    public class UpdateOrderUC
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateOrderUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<Order>> UpdateStatusOrder(int orderID, string status)
        {
            try
            {
                Order? order = await this.unitOfWork.OrderRepository().GetById(orderID);
                if (order == null)
                {
                    return ServiceResult<Order>.Failure(
                        "Order not found.",
                        ServiceErrorType.NotFound);
                }
                order.Status = status;
                await this.unitOfWork.Commit();
                return ServiceResult<Order>.Success(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return ServiceResult<Order>.Failure(
                    "An error occurred while updating the order status.",
                    ServiceErrorType.InternalError);
            }
        }
         
    }
}
