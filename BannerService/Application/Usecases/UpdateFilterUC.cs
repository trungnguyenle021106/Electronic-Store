
using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace BannerService.Application.Usecases
{
    public class UpdateFilterUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateFilterUC(ContentManagementContext bannerContext)
        {
            this.unitOfWork = new ContentManagementUnitOfWork(bannerContext);
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
