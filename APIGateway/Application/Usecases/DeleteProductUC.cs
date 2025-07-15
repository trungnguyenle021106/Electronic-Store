

namespace APIGateway.Application.Usecases
{
    public class DeleteProductUC
    {
        public DeleteProductUC()
        {
      
        }

        //public async Task<DeletionResult<ProductPropertyDetail>> DeleteProductPropertyDetail(ProductPropertyDetail productPropertyDetail)
        //{
        //    try
        //    {
        //        if (productPropertyDetail == null)
        //        {
        //            return DeletionResult<ProductPropertyDetail>.Failure(
        //                "ProductPropertyDetail cannot be null.",
        //                DeletionErrorType.ValidationError
        //            );
        //        }

        //        IQueryable<ProductPropertyDetail> query = this.unitOfWork.ProductPropertyDetailRepository().
        //            GetByCompositeKey(productPropertyDetail.ProductID, "ProductID",
        //            productPropertyDetail.ProductPropertyID, "ProductPropertyID");

        //        ProductPropertyDetail? existingDetail = await query.FirstOrDefaultAsync();
        //        if (existingDetail == null)
        //        {
        //            return DeletionResult<ProductPropertyDetail>.Failure(
        //                $"ProductPropertyDetail not found.",
        //                DeletionErrorType.NotFound
        //            );
        //        }

        //        this.unitOfWork.ProductPropertyDetailRepository().Remove(productPropertyDetail);
        //        await this.unitOfWork.Commit();

        //        return DeletionResult<ProductPropertyDetail>.Success(productPropertyDetail);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi xóa ProductPropertyDetail: {ex.Message}");
        //        return DeletionResult<ProductPropertyDetail>.Failure(
        //            "An internal error occurred during deletion.",
        //            DeletionErrorType.InternalError
        //        );
        //    }
        //}

       
    }
}
