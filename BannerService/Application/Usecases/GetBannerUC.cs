using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

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
                IQueryable<Banner> query = this.unitOfWork.BannerRepository().GetByIdQueryable(bannerID);
                Banner? banner = await query.FirstOrDefaultAsync();
                if (banner == null) { return null; }
                return banner;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy banner id : {bannerID} lỗi : {ex}");
                return null;
            }
        }

        public async Task<Banner?> GetBannerByPosition(string position)
        {
            try
            {
                IQueryable<Banner> query = this.unitOfWork.BannerRepository().GetByFieldQueryable("Position", position);
                Banner? banner = await query.FirstOrDefaultAsync();
                if (banner == null) { return null; }
                return banner;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy banner tại position : {position} lỗi : {ex}");
                return null;
            }
        }

        public async Task<List<Banner>?> GetAllBanner()
        {
            try
            {
                IQueryable<Banner> query = this.unitOfWork.BannerRepository().GetAll();
                List<Banner> list = await query.ToListAsync();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách banner : {ex}");
                return null;
            }
        }


    }
}
