using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;

namespace BannerService.Application.Usecases
{
    public class CreateBannerUC
    {
        private readonly IUnitOfWork unitOfWork;
        public CreateBannerUC(BannerContext bannerContext)
        {
            this.unitOfWork = new BannerUnitOfWork(bannerContext);
        }

        public async Task<Banner?> CreateBanner(Banner banner)
        {
            try
            {
                Banner newBanner = await this.unitOfWork.BannerRepository().Add(banner);
                await this.unitOfWork.Commit();
                return newBanner;   
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo banner : " + ex.ToString());
                return null;
            }
        }
    }
}
