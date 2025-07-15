
namespace APIGateway.Application.Usecases
{
    public class GetProductUC
    {
        public GetProductUC()
        {
 
        }

        //public async Task<QueryResult<Product>> GetProductByID(int id)
        //{
        //    try
        //    {
        //        if (id <= 0)
        //        {
        //            return QueryResult<Product>.Failure("Product ID is invalid.", RetrievalErrorType.ValidationError);
        //        }

        //        IQueryable<Product> query = this.unitOfWork.ProductRepository().GetByIdQueryable(id);
        //        Product? product = await query.FirstOrDefaultAsync();
        //        if (product == null)
        //        {
        //            return QueryResult<Product>.Failure($"Product with ID : '{product.ID}' is not exist.",
        //                RetrievalErrorType.NotFound);
        //        }

        //        return QueryResult<Product>.Success(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi lấy sản phẩm có id : {id}, lỗi : {ex.Message}");
        //        return QueryResult<Product>.Failure("An unexpected internal error occurred while get product.",
        //              RetrievalErrorType.InternalError);
        //    }
        //}

    }
}
