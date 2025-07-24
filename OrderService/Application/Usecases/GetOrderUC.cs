using CommonDto.HandleErrorResult;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.UnitOfWork;
using OrderService.Domain.DTO.Response;
using OrderService.Domain.Entities;
using OrderService.Domain.Interface.UnitOfWork;
using OrderService.Infrastructure.Data.DBContext;
using OrderService.Infrastructure.DTO;
using OrderService.Infrastructure.Service;

namespace OrderService.Application.Usecases
{
    public class GetOrderUC
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ProductService productService;
        private readonly HandleServiceError handleServiceError;
        public GetOrderUC(IUnitOfWork _UnitOfWork, ProductService productService, HandleServiceError handleServiceError)
        {
            this._UnitOfWork = _UnitOfWork;
            this.productService = productService;
            this.handleServiceError = handleServiceError;
        }


        public async Task<ServiceResult<OrderItem>> GetOrderItemOfOrder(int orderID)
        {
            if (orderID <= 0)
            {
                return ServiceResult<OrderItem>.Failure(
                    "Invalid order ID.",
                    ServiceErrorType.ValidationError);
            }
            try
            {
                List<OrderDetail> orderDetails = await this._UnitOfWork.OrderDetailRepository().GetAll()
                    .Where(od => od.OrderID == orderID).ToListAsync();
                if (orderDetails == null || !orderDetails.Any())
                {
                    return ServiceResult<OrderItem>.Failure(
                        "No order details found for the specified order ID.",
                        ServiceErrorType.NotFound);
                }
                List<int> productIDs = orderDetails.Select(od => od.ProductID).ToList();
                ServiceResult<Product> result = await this.productService.CheckExistProducts(productIDs);
                if (!result.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                   MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "CheckExistProducts");
                    return ServiceResult<OrderItem>.Failure(
                        errorResult.Message,
                        errorResult.ServiceErrorType);
                }

                if(result.ListItem == null || !result.ListItem.Any())
                {
                    return ServiceResult<OrderItem>.Failure(
                        "No products found for the specified order details.",
                        ServiceErrorType.NotFound);
                }

                List<OrderItem> list = orderDetails.Join(
                    result.ListItem,
                    od => od.ProductID,
                    p => p.ID,
                    (od, p) => new 
                    {
                        OrderDetail = od, Product = p
                    }).Select(joined => new OrderItem
                    {
                        Product = joined.Product,
                        Quantity = joined.OrderDetail.Quantity,
                        TotalPrice = joined.OrderDetail.TotalPrice,
                    }).ToList();

                return ServiceResult<OrderItem>.Success(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm của đơn hàng theo OrderID {orderID}: {ex.Message}");
                return ServiceResult<OrderItem>.Failure(
                    "An error occurred while retrieving the products of the order.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetProductOfOrder(int orderID)
        {
            try
            {
                List<OrderDetail> orderDetails = await this._UnitOfWork.OrderDetailRepository().GetAll()
                    .Where(od => od.OrderID == orderID).ToListAsync();
                if (orderDetails == null || !orderDetails.Any())
                {
                    return ServiceResult<Product>.Failure(
                        "No order details found for the specified order ID.",
                        ServiceErrorType.NotFound);
                }
                List<int> productIDs = orderDetails.Select(od => od.ProductID).ToList();
                ServiceResult<Product> result = await this.productService.CheckExistProducts(productIDs);
                if (!result.IsSuccess)
                {
                    ErrorServiceResult errorResult = this.handleServiceError.
                   MapServiceError(result.ServiceErrorType ?? ServiceErrorType.InternalError, "CheckExistProducts");
                    return ServiceResult<Product>.Failure(
                        errorResult.Message,
                        errorResult.ServiceErrorType);
                }
                return ServiceResult<Product>.Success(result.ListItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy sản phẩm của đơn hàng theo OrderID {orderID}: {ex.Message}");
                return ServiceResult<Product>.Failure(
                    "An error occurred while retrieving the products of the order.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Order>> GetOrderByID(int id)
        {
            try
            {
                Order? order = await this._UnitOfWork.OrderRepository().GetById(id);
                if (order == null)
                {
                    return ServiceResult<Order>.Failure(
                        "Order not found.",
                        ServiceErrorType.NotFound);
                }
                return ServiceResult<Order>.Success(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy Order theo ID {id}: {ex.Message}");
                return ServiceResult<Order>.Failure(
                    "An error occurred while retrieving the order.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<Order>>> GetPagedOrder(int page, int pageSize, string? searchText, bool? isIncrease)
        {
            try
            {
                IQueryable<Order> query = this._UnitOfWork.OrderRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.Trim().ToLower();

                    DateTime searchDate;
                    bool isDate = DateTime.TryParse(searchText, out searchDate); 

                    int searchInt;
                    bool isInt = int.TryParse(searchText, out searchInt);

                    float searchFloat;
                    bool isFloat = float.TryParse(searchText, out searchFloat);

                    query = query.Where(order =>
                        (order.Status != null && order.Status.ToLower().Contains(searchLower)) ||
                        (isInt && order.CustomerID == searchInt) ||
                        (isFloat && Math.Abs(order.Total - searchFloat) < 0.001f) ||
                        (isDate && order.OrderDate.Date == searchDate.Date)
                    );
                }

                if (isIncrease != null && isIncrease == true)
                {
                    query = query.OrderBy(order => order.ID);
                }
                else if (isIncrease != null && isIncrease == false)
                {
                    query = query.OrderByDescending(order => order.ID);
                }
                else
                {
                    query = query.OrderBy(order => order.ID);
                }

                int totalCount = await query.CountAsync();

                List<Order> list = new List<Order>();
                // 4. Kiểm tra trang hợp lệ

                if (page < 1)
                {
                    page = 1;
                }
                query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                // 6. Áp dụng phân trang (Skip và Take)
                list = await query
                   .Skip((int)((page - 1) * pageSize))
                   .Take((int)pageSize)
                   .ToListAsync();

                return ServiceResult<PagedResult<Order>>.Success(new PagedResult<Order>
                {
                    Items = list,
                    Page = (int)page,
                    PageSize = (int)pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách Order, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<Order>>.Failure(
                    "An error occurred while retrieving the orders.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<OrderDetail>> GetOrderDetailOfOrder(int orderID)
        {
            try
            {
                List<OrderDetail> orderDetails = await this._UnitOfWork.OrderDetailRepository().GetAll()
                    .Where(od => od.OrderID == orderID).ToListAsync();
                if (orderDetails == null || !orderDetails.Any())
                {
                    return ServiceResult<OrderDetail>.Failure(
                        "No order details found for the specified order ID.",
                        ServiceErrorType.NotFound);
                }
                return ServiceResult<OrderDetail>.Success(orderDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy chi tiết đơn hàng theo OrderID {orderID}: {ex.Message}");
                return ServiceResult<OrderDetail>.Failure(
                    "An error occurred while retrieving the order details.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<OrderDetail>> GetOrderDetailOfOrder(Order order)
        {
            try
            {
                List<OrderDetail> orderDetails = await this._UnitOfWork.OrderDetailRepository().GetAll()
                    .Where(od => od.OrderID == order.ID).ToListAsync();
                if (orderDetails == null || !orderDetails.Any())
                {
                    return ServiceResult<OrderDetail>.Failure(
                        "No order details found for the specified order ID.",
                        ServiceErrorType.NotFound);
                }
                return ServiceResult<OrderDetail>.Success(orderDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy chi tiết đơn hàng theo OrderID {order.ID}: {ex.Message}");
                return ServiceResult<OrderDetail>.Failure(
                    "An error occurred while retrieving the order details.",
                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Order>> GetOrdersByCustomerID(int customerID)
        {
            try
            {
                List<Order> orders = await this._UnitOfWork.OrderRepository().GetAll()
                    .Where(o => o.CustomerID == customerID).ToListAsync();
                if (orders == null || !orders.Any())
                {
                    return ServiceResult<Order>.Failure(
                        "No orders found for the specified customer ID.",
                        ServiceErrorType.NotFound);
                }
                return ServiceResult<Order>.Success(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy đơn hàng theo CustomerID {customerID}: {ex.Message}");
                return ServiceResult<Order>.Failure(
                    "An error occurred while retrieving the orders for the customer.",
                    ServiceErrorType.InternalError);
            }

        }
    }
}
