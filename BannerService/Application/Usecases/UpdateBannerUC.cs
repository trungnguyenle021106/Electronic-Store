
using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace BannerService.Application.Usecases
{
    public class UpdateBannerUC
    {
        private readonly IUnitOfWork unitOfWork;
        public UpdateBannerUC(BannerContext bannerContext)
        {
            this.unitOfWork = new BannerUnitOfWork(bannerContext);
        }

        public async Task<Banner?> UpdateBanner(int bannerID, Banner newBanner)
        {
            try
            {
                IQueryable<Banner> query = this.unitOfWork.BannerRepository().GetByIdQueryable(bannerID);
                Banner? banner = await query.FirstOrDefaultAsync();
                if(banner == null) {return null;}

                banner.Position = newBanner.Position;
                banner.Image = newBanner.Image;
                this.unitOfWork.BannerRepository().Update(banner);
                await this.unitOfWork.Commit();
                return banner;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật banner : " + ex.Message);
                return null;
            }
        }
    }
}
