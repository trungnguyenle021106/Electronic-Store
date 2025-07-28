using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.UnitOfWork;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.Data.DBContext;
using OrderService.Infrastructure.DTO;
using OrderService.Infrastructure.Service;

namespace OrderService.Application.Usecases
{
    public class UpdateOrderUC
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;
        public UpdateOrderUC(IUnitOfWork unitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this.unitOfWork = unitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
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

                if (status.Equals("Đã giao đi") || status.Equals("Đã hủy"))
                {
                    List<OrderProduct> orderProducts = await this.unitOfWork.OrderDetailRepository().GetAll().
                  Where(orderDetail => orderDetail.OrderID == order.ID).Select(orderDetail =>
                  new OrderProduct
                  {
                      ProductID = orderDetail.ProductID,
                      Quantity = orderDetail.Quantity
                  }).
                  ToListAsync();

                    if (status.Equals("Đã giao đi"))
                    {
                        foreach (OrderProduct orderProduct in orderProducts)
                        {
                            orderProduct.Quantity = orderProduct.Quantity * -1;
                        }
                    }

                    ServiceResult<Product> result = await this.productService.UpdateProductQuantity(orderProducts);
                    if (!result.IsSuccess)
                    {
                        ErrorServiceResult errorResult = this.handleServiceError.
                          MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "UpdateProductQuantity");
                        return ServiceResult<Order>.Failure(
                            errorResult.Message,
                            errorResult.ServiceErrorType);
                    }
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
