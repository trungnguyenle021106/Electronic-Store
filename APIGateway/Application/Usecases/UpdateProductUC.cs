
namespace APIGateway.Application.Usecases
{
    public class UpdateProductUC
    {
        public UpdateProductUC()
        {
           
        }

        //public async Task<UpdateResult<Product>> UpdateProduct(int productID, Product newProduct)
        //{
        //    try
        //    {
        //        IQueryable<Product> productQuery = this.unitOfWork.ProductRepository().GetByIdQueryable(productID);
        //        Product? existingProduct = await productQuery.FirstOrDefaultAsync();
        //        if (existingProduct == null)
        //        {
        //            return UpdateResult<Product>.Failure("Product not found.", UpdateErrorType.NotFound);
        //        }

        //        existingProduct.Description = newProduct.Description;
        //        existingProduct.Price = newProduct.Price;
        //        existingProduct.Quantity = newProduct.Quantity;
        //        existingProduct.Status = newProduct.Status;
        //        existingProduct.Name = newProduct.Name;
        //        existingProduct.ProductTypeID = newProduct.ProductTypeID;
        //        existingProduct.ProductBrandID = newProduct.ProductBrandID;

        //        await this.unitOfWork.Commit();
        //        return UpdateResult<Product>.Success(existingProduct);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Lỗi cập nhật Product: " + ex.ToString());
        //        return UpdateResult<Product>.Failure("An internal error occurred during product update.", UpdateErrorType.InternalError);
        //    }
        //}

      
   }
}
