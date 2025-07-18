﻿using Azure;
using CommonDto.ResultDTO;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.DTO;
using ProductService.Domain.Entities;
using ProductService.Domain.Interface.UnitOfWork;


namespace ProductService.Application.Usecases
{
    public class GetProductUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetProductUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<ProductProperty>> GetAllPropertiesOfProduct(int id)
        {

            if (id <= 0)
            {
                return ServiceResult<ProductProperty>.Failure("Product ID is invalid.", ServiceErrorType.ValidationError);
            }
            try
            {
                IQueryable<ProductProperty> query = this.unitOfWork.ProductPropertyRepository().GetAll();
                query = query.Join(this.unitOfWork.ProductPropertyDetailRepository().GetAll(),
                    productProperty => productProperty.ID,
                    productPropertyDetail => productPropertyDetail.ProductPropertyID,
                    (productProperty, productPropertyDetail) => new
                    {
                        ProductPropertyDetail = productPropertyDetail,
                        ProductProperty = productProperty
                    }
                    ).
                    Where(joined => joined.ProductPropertyDetail.ProductID == id).
                    Select(joined => joined.ProductProperty);

                List<ProductProperty>? list = await query.ToListAsync();

                if (list == null)
                {
                    return ServiceResult<ProductProperty>.Failure($"Product with ID : '{id}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                return ServiceResult<ProductProperty>.Success(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
                return ServiceResult<ProductProperty>.Failure("An unexpected internal error occurred while GetAllPropertiesOfProduct.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<ProductProperty>>> GetPagedProductProperties(
          int page,
          int pageSize,
          string? searchText, // Dùng string? cho phép null
          string? filter)    // Dùng string? cho phép null
        {
            try
            {
                IQueryable<ProductProperty> query = this.unitOfWork.ProductPropertyRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.ToLower();

                    // Điều chỉnh các cột của ProductProperty mà bạn muốn tìm kiếm
                    query = query.Where(pp =>
                        (pp.Name != null && pp.Name.ToLower().Contains(searchLower)) ||
                        (pp.Description != null && pp.Description.ToLower().Contains(searchLower))
                    // Thêm các cột string khác của ProductProperty nếu có
                    );
                }

                // 2. Áp dụng lọc (Filter) theo tên thuộc tính (ProductProperty.Name)
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string filterLower = filter.ToLower();
                    // Lọc trực tiếp trên cột Name của ProductProperty
                    query = query.Where(pp => pp.Name != null && pp.Name.ToLower().Contains(filterLower));
                }

                // 3. Lấy tổng số lượng bản ghi sau khi áp dụng tất cả các điều kiện lọc và tìm kiếm
                int totalCount = await query.CountAsync();

                // 4. Kiểm tra trang hợp lệ
                if (page < 1)
                {
                    page = 1;
                }

                // 5. Áp dụng sắp xếp mặc định (nếu bạn không có tham số sắp xếp từ frontend)
                // Luôn sắp xếp trước khi phân trang để đảm bảo kết quả nhất quán
                query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                // 6. Áp dụng phân trang (Skip và Take)
                List<ProductProperty>? list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return ServiceResult<PagedResult<ProductProperty>>.Success(new PagedResult<ProductProperty>
                {
                    Items = list,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy thuộc tính sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<ProductProperty>>.Failure("An unexpected internal error occurred while GetPagedProductProperties.",
                                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<ProductDTO>>> GetPagedProducts(int Page, int PageSize, string? searchText,
            string? filterBrand, string? filterType, string? filterStatus)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.ToLower();
                    query = query.
                        Join(this.unitOfWork.ProductBrandRepository().GetAll(),
                            product => product.ProductBrandID,
                            productBrand => productBrand.ID,
                            (product, productBrand) => new { Product = product, ProductBrand = productBrand }
                        ).
                        Join(this.unitOfWork.ProductTypeRepository().GetAll(),
                            joined => joined.Product.ProductTypeID,
                            productType => productType.ID,
                            (joined, productType) => new
                            {
                                Product = joined.Product,          // Lấy Product từ đối tượng 'joined' trước đó
                                ProductBrand = joined.ProductBrand, // Lấy ProductBrand từ đối tượng 'joined' trước đó
                                ProductType = productType          // Thêm ProductType mới
                            }
                        ).
                        Where(item =>
                        (item.Product.Name != null && item.Product.Name.ToLower().Contains(searchLower)) ||
                        (item.Product.Description != null && item.Product.Description.ToLower().Contains(searchLower)) ||
                        (item.Product.Quantity.ToString().Contains(searchLower)) ||
                        (item.Product.Price.ToString().Contains(searchLower)) ||
                        (item.Product.Status != null && item.Product.Status.ToLower().Contains(searchLower)) ||
                        (item.ProductType.Name != null && item.ProductType.Name.ToLower().Contains(searchLower)) ||
                        (item.ProductBrand.Name != null && item.ProductBrand.Name.ToLower().Contains(searchLower))
                    ).Select(item => item.Product);
                }

                if (!string.IsNullOrWhiteSpace(filterType))
                {
                    string filterLower = filterType.ToLower();
                    query = query.
                       Join(this.unitOfWork.ProductTypeRepository().GetAll(),
                           product => product.ProductTypeID,
                           producType => producType.ID,
                           (product, producType) => new { Product = product, ProductType = producType }
                       ).
                       Where(
                       item => item.ProductType.Name != null && item.ProductType.Name.ToLower().Contains(filterType)
                   ).Select(item => item.Product);
                }

                if (!string.IsNullOrWhiteSpace(filterBrand))
                {
                    string filterLower = filterBrand.ToLower();
                    query = query.
                       Join(this.unitOfWork.ProductBrandRepository().GetAll(),
                           product => product.ProductBrandID,
                           productBrrand => productBrrand.ID,
                           (product, productBrrand) => new { Product = product, ProductBrand = productBrrand }
                       ).
                       Where(
                       item => item.ProductBrand.Name != null && item.ProductBrand.Name.ToLower().Contains(filterBrand)
                   ).Select(item => item.Product);
                }

                if (!string.IsNullOrWhiteSpace(filterStatus))
                {
                    string filterLower = filterStatus.ToLower();
                    query = query.
                       Where(item => item.Status != null && item.Status.ToLower().Contains(filterStatus));
                }

                int TotalCount = await query.CountAsync();

                if (Page < 1)
                {
                    Page = 1;
                }

                List<ProductDTO>? list = await query
                    .Join(this.unitOfWork.ProductTypeRepository().GetAll(),
                        product => product.ProductTypeID,
                        productType => productType.ID,
                        (product, productType) => new { Product = product, ProductType = productType }
                    )
                    .Join(this.unitOfWork.ProductBrandRepository().GetAll(),
                        item => item.Product.ProductBrandID,
                        productBrand => productBrand.ID,
                        (item, productBrand) => new ProductDTO
                        {
                            ID = item.Product.ID,
                            Name = item.Product.Name,
                            Quantity = item.Product.Quantity,
                            Image = item.Product.Image,
                            Price = item.Product.Price,
                            Status = item.Product.Status,
                            ProductTypeName = item.ProductType.Name,
                            ProductBrandName = productBrand.Name
                        }
                    )
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                return ServiceResult<PagedResult<ProductDTO>>.Success(new PagedResult<ProductDTO>
                {
                    Items = list,
                    Page = Page,
                    PageSize = PageSize,
                    TotalCount = TotalCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<ProductDTO>>.Failure("An unexpected internal error occurred while GetPagedProducts.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<string>> GetAllUniquePropertyNames()
        {
            try
            {
                List<string> propertyNames = await this.unitOfWork.ProductPropertyRepository()
                                                     .GetAll() // Lấy tất cả ProductProperty
                                                     .Where(pp => pp.Name != null) // Đảm bảo tên không null
                                                     .Select(pp => pp.Name!) // Chỉ chọn cột Name (dấu ! để khẳng định không null)
                                                     .Distinct() // Lấy các giá trị duy nhất
                                                     .OrderBy(name => name) // Sắp xếp theo thứ tự bảng chữ cái (tùy chọn)
                                                     .ToListAsync(); // Thực thi truy vấn và trả về List<string>

                return ServiceResult<string>.Success(propertyNames);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách tên thuộc tính duy nhất, lỗi: {ex.Message}");
                return ServiceResult<string>.Failure("An unexpected internal error occurred while getting unique property names.",
                                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetProductByID(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<Product>.Failure("Product ID is invalid.", ServiceErrorType.ValidationError);
            }
            try
            {
                Product? product = await this.unitOfWork.ProductRepository().GetById(id);


                if (product == null)
                {
                    return ServiceResult<Product>.Failure($"Product with ID : '{id}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                return ServiceResult<Product>.Success(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
                return ServiceResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetAllProductByType(string productTypeName)
        {
            try
            {
                if (productTypeName != null)
                {
                    return ServiceResult<Product>.Failure("Product type is invalid.", ServiceErrorType.ValidationError);
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query
                .Join(
                  this.unitOfWork.ProductTypeRepository().GetAll(),
                  product => product.ProductTypeID,
                  productType => productType.ID,
                  (product, productType) => new { Product = product, ProductType = productType }
                  )
                .Where(item => item.ProductType.Name == productTypeName)
                .Select(item => item.Product);

                List<Product>? products = await query.ToListAsync();
                if (products == null)
                {
                    return ServiceResult<Product>.Failure($"Product with type : '{productTypeName}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                return ServiceResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có type : {productTypeName}, lỗi : {ex.Message}");
                return ServiceResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetAllProductByBrand(string productBrandName)
        {
            try
            {
                if (productBrandName != null)
                {
                    return ServiceResult<Product>.Failure("Product type is invalid.", ServiceErrorType.ValidationError);
                }

                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query
                .Join(
                  this.unitOfWork.ProductBrandRepository().GetAll(),
                  product => product.ProductBrandID,
                  productBrand => productBrand.ID,
                  (product, productBrand) => new { Product = product, ProductBrand = productBrand }
                  )
                .Where(item => item.ProductBrand.Name == productBrandName)
                .Select(item => item.Product);

                List<Product>? products = await query.ToListAsync();
                if (products == null)
                {
                    return ServiceResult<Product>.Failure($"Product with name : '{productBrandName}' is not exist.",
                        ServiceErrorType.NotFound);
                }

                return ServiceResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy sản phẩm có name : {productBrandName}, lỗi : {ex.Message}");
                return ServiceResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<ProductType>>> GetPagedProductTypes(int page, int pageSize, string? searchText, string? filter)
        {
            try
            {
                IQueryable<ProductType> query = this.unitOfWork.ProductTypeRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.ToLower();

                    // Điều chỉnh các cột của ProductProperty mà bạn muốn tìm kiếm
                    query = query.Where(pp =>
                        (pp.Name != null && pp.Name.ToLower().Contains(searchLower))

                    );
                }

                // 2. Áp dụng lọc (Filter) theo tên thuộc tính (ProductProperty.Name)
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string filterLower = filter.ToLower();
                    // Lọc trực tiếp trên cột Name của ProductProperty
                    query = query.Where(pp => pp.Name != null && pp.Name.ToLower().Contains(filterLower));
                }

                // 3. Lấy tổng số lượng bản ghi sau khi áp dụng tất cả các điều kiện lọc và tìm kiếm
                int totalCount = await query.CountAsync();

                // 4. Kiểm tra trang hợp lệ
                if (page < 1)
                {
                    page = 1;
                }

                // 5. Áp dụng sắp xếp mặc định (nếu bạn không có tham số sắp xếp từ frontend)
                // Luôn sắp xếp trước khi phân trang để đảm bảo kết quả nhất quán
                query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                // 6. Áp dụng phân trang (Skip và Take)
                List<ProductType>? list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return ServiceResult<PagedResult<ProductType>>.Success(new PagedResult<ProductType>
                {
                    Items = list,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy brand sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<ProductType>>.Failure("An unexpected internal error occurred while GetPagedProductType.",
                                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<PagedResult<ProductBrand>>> GetPagedProductBrands(int page, int pageSize, string? searchText, string? filter)
        {
            try
            {
                IQueryable<ProductBrand> query = this.unitOfWork.ProductBrandRepository().GetAll();

                // 1. Áp dụng tìm kiếm (Search) vào tất cả các cột có thể
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchLower = searchText.ToLower();

                    // Điều chỉnh các cột của ProductProperty mà bạn muốn tìm kiếm
                    query = query.Where(pp =>
                        (pp.Name != null && pp.Name.ToLower().Contains(searchLower))

                    );
                }

                // 2. Áp dụng lọc (Filter) theo tên thuộc tính (ProductProperty.Name)
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string filterLower = filter.ToLower();
                    // Lọc trực tiếp trên cột Name của ProductProperty
                    query = query.Where(pp => pp.Name != null && pp.Name.ToLower().Contains(filterLower));
                }

                // 3. Lấy tổng số lượng bản ghi sau khi áp dụng tất cả các điều kiện lọc và tìm kiếm
                int totalCount = await query.CountAsync();

                // 4. Kiểm tra trang hợp lệ
                if (page < 1)
                {
                    page = 1;
                }

                // 5. Áp dụng sắp xếp mặc định (nếu bạn không có tham số sắp xếp từ frontend)
                // Luôn sắp xếp trước khi phân trang để đảm bảo kết quả nhất quán
                query = query.OrderBy(pp => pp.ID); // Sắp xếp mặc định theo ID

                // 6. Áp dụng phân trang (Skip và Take)
                List<ProductBrand>? list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return ServiceResult<PagedResult<ProductBrand>>.Success(new PagedResult<ProductBrand>
                {
                    Items = list,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount // Sử dụng totalCount đã được lọc/tìm kiếm
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy brand sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<PagedResult<ProductBrand>>.Failure("An unexpected internal error occurred while GetPagedProductBrand.",
                                    ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetAllProductByTypeAndProperty(string productTypeName, ProductProperty productProperty)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query.
                    Join(this.unitOfWork.ProductPropertyDetailRepository().GetAll(),
                    product => product.ID,
                    productPropertyDetail => productPropertyDetail.ProductID,
                    (product, productPropertyDetail) => new
                    {
                        Product = product,
                        ProductPropertyDetail = productPropertyDetail
                    }).
                    Join(this.unitOfWork.ProductPropertyRepository().GetAll(),
                      joinedItem => joinedItem.ProductPropertyDetail.ProductPropertyID,
                      productProperty => productProperty.ID,
                      (joinedItem, productProperty) => new
                      {
                          Product = joinedItem.Product,
                          ProductPropertyDetail = joinedItem.ProductPropertyDetail,
                          ProductProperty = productProperty
                      }).
                      Where(item =>
                            item.ProductProperty.Name == productTypeName &&
                            item.ProductProperty.Name == productProperty.Name &&
                            item.ProductProperty.Description == productProperty.Description
                      ).
                      Select(item => item.Product);
                List<Product> products = await query.ToListAsync();

                return ServiceResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      ServiceErrorType.InternalError);
            }
        }

        public async Task<ServiceResult<Product>> GetAllProductByTypeAndBrand(string productTypeName, string productBrandName)
        {
            try
            {
                IQueryable<Product> query = this.unitOfWork.ProductRepository().GetAll();
                query = query.
                    Join(this.unitOfWork.ProductBrandRepository().GetAll(),
                    product => product.ProductBrandID,
                    productBrand => productBrand.ID,
                    (product, productBrand) => new
                    {
                        Product = product,
                        ProductBrand = productBrand
                    }).
                      Join(this.unitOfWork.ProductTypeRepository().GetAll(),
                    joinedItem => joinedItem.Product.ProductTypeID,
                    productType => productType.ID,
                    (joinedItem, productType) => new
                    {
                        Product = joinedItem.Product,
                        ProductBrand = joinedItem.ProductBrand,
                        ProductType = productType
                    }).
                      Where(item =>
                            item.ProductBrand.Name == productBrandName &&
                            item.ProductType.Name == productTypeName
                      ).
                      Select(item => item.Product);
                List<Product> products = await query.ToListAsync();

                return ServiceResult<Product>.Success(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách sản phẩm, lỗi : {ex.Message}");
                return ServiceResult<Product>.Failure("An unexpected internal error occurred while get product.",
                      ServiceErrorType.InternalError);
            }
        }
    }
}
