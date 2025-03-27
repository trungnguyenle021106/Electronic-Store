
using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;

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
                Banner banner = await this.unitOfWork.BannerRepository().GetById(bannerID);
                banner.Position = newBanner.Position;
                banner.Image = newBanner.Image;
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
