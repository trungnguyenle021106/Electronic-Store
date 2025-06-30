using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using ContentManagementService.Domain.Entities;

namespace BannerService.Application.Usecases
{
    public class CreateFilterUC
    {
        private readonly IUnitOfWork unitOfWork;
        public CreateFilterUC(ContentManagementContext filterContext)
        {
            this.unitOfWork = new ContentManagementUnitOfWork(filterContext);
        }

        public async Task<Filter?> CreateFilter(Filter filter)
        {
            try
            {
                Filter newfilter = await this.unitOfWork.FilterRepository().Add(filter);
                await this.unitOfWork.Commit();
                return newfilter;   
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo filter : " + ex.ToString());
                return null;
            }
        }

        public async Task<FilterDetail?> CreateFilterDetail(FilterDetail filterDetail)
        {
            try
            {
                FilterDetail newFilterDetail = await this.unitOfWork.FilterDetailRepository().Add(filterDetail);
                await this.unitOfWork.Commit();
                return newFilterDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi tạo filter detail : " + ex.ToString());
                return null;
            }
        }
    }
}
