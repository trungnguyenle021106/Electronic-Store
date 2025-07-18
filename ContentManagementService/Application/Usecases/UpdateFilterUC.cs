
using ContentManagementService.Application.UnitOfWork;
using ContentManagementService.Domain.Entities;
using ContentManagementService.Domain.Interface.UnitOfWork;
using ContentManagementService.Infrastructure.Data.DBContext;
using Microsoft.EntityFrameworkCore;

namespace ContentManagementService.Application.Usecases
{
    public class UpdateFilterUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateFilterUC(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        //public async Task<Filter?> UpdateBanner(int bannerID, Filter newBanner)
        //{
        //    try
        //    {
        //        IQueryable<Filter> query = this.unitOfWork.BannerRepository().GetByIdQueryable(bannerID);
        //        Filter? banner = await query.FirstOrDefaultAsync();
        //        if(banner == null) {return null;}

        //        banner.Position = newBanner.Position;
        //        banner.Image = newBanner.Image;
        //        this.unitOfWork.BannerRepository().Update(banner);
        //        await this.unitOfWork.Commit();
        //        return banner;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Lỗi cập nhật banner : " + ex.Message);
        //        return null;
        //    }
        //}
    }
}
