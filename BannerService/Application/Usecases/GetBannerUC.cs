using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;

namespace BannerService.Application.Usecases
{
    public class GetBannerUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetBannerUC(BannerContext bannerContext)
        {
            this.unitOfWork = new BannerUnitOfWork(bannerContext);
        }

        public async Task<Banner?> GetBannerByID(int bannerID)
        {
            try
            {
                return await this.unitOfWork.BannerRepository().GetById(bannerID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy banner id : {bannerID} lỗi : {ex}");
                return null;
            }
        }

        public async Task<List<Banner>?> GetAllBanner()
        {
            try
            {
                return (List<Banner>)await this.unitOfWork.BannerRepository().GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách banner : {ex}");
                return null;
            }
        }
    }
}
