using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.DTO;
using OrderService.Infrastructure.Service;

namespace OrderService.Application.Usecases
{
    public class CreateOrderUC
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;

        public CreateOrderUC(IUnitOfWork _UnitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this._UnitOfWork = _UnitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }

        public async Task<ServiceResult<Order>> CreateOrder(int customerID, List<OrderDetail> orderDetails)
        {
            if (orderDetails == null || !orderDetails.Any())
            {
                return ServiceResult<Order>.Failure(
                    "Product IDs cannot be null or empty.",
                    ServiceErrorType.ValidationError);
            }

            if (customerID <= 0)
            {
                return ServiceResult<Order>.Failure(
                    "Invalid Customer ID.",
                    ServiceErrorType.ValidationError);
            }


            try
            {
                ServiceResult<Product> result = await this.productService.CheckExistProducts(orderDetails.Select(od => od.ProductID).ToList());
                if (!result.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                      MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "CheckExistProducts");
                    return ServiceResult<Order>.Failure(
                        errorResult.Message,
                        errorResult.ServiceErrorType);
                }

                List<Product> existProducts = result.ListItem;
                if (existProducts == null || !existProducts.Any())
                {
                    return ServiceResult<Order>.Failure(
                        "No valid products found for the provided IDs.",
                        ServiceErrorType.NotFound);
                }

                List<OrderDetail> listExistOrderDetail = orderDetails.
                    Where(orderDetails => existProducts.
                        Any(p => p.ID == orderDetails.ProductID)
                    ).ToList();



                if (listExistOrderDetail == null || !listExistOrderDetail.Any())
                {
                    return ServiceResult<Order>.Failure(
                        "No valid order details found for the provided product IDs.",
                        ServiceErrorType.NotFound);
                }

                using (var transaction = await this._UnitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        DateTime OrderDate = DateTime.Now;
                        float Total = listExistOrderDetail.Sum(item => item.TotalPrice);
                        string Status = "Đang chờ xử lý";

                        Order order = await this._UnitOfWork.OrderRepository().Add(new Order
                        {
                            CustomerID = customerID,
                            OrderDate = OrderDate,
                            Total = Total,
                            Status = Status
                        });
                        await this._UnitOfWork.Commit();

                        foreach (OrderDetail orderDetail in listExistOrderDetail)
                        {
                            orderDetail.OrderID = order.ID;
                        }

                        await this._UnitOfWork.OrderDetailRepository().AddRangeAsync(listExistOrderDetail);
                        await this._UnitOfWork.Commit();
                        await this._UnitOfWork.CommitTransactionAsync(transaction);

                        return ServiceResult<Order>.Success(order);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Transaction error: {ex.Message}");
                        await this._UnitOfWork.RollbackAsync(transaction);
                        return ServiceResult<Order>.Failure(
                            "Failed to create order due to transaction error.",
                            ServiceErrorType.InternalError);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                return ServiceResult<Order>.Failure(
                    "An error occurred while creating the order.",
                    ServiceErrorType.InternalError);
            }
        }
    }
}
