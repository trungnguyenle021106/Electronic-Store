using BannerService.Application.UnitOfWork;
using BannerService.Domain.Entities;
using BannerService.Domain.Interface.UnitOfWork;
using BannerService.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace BannerService.Application.Usecases
{
    public class GetFilterUC
    {
        private readonly IUnitOfWork unitOfWork;
        public GetFilterUC(ContentManagementContext filterContext)
        {
            this.unitOfWork = new ContentManagementUnitOfWork(filterContext);
        }

        public async Task<Filter?> GetFilterByID(int filterID)
        {
            try
            {
                IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetByIdQueryable(filterID);
                Filter? filter = await query.FirstOrDefaultAsync();
                if (filter == null) { return null; }
                return filter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy filter id : {filterID} lỗi : {ex}");
                return null;
            }
        }

        public async Task<Filter?> GetFilterByPosition(string position)
        {
            try
            {
                IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetByFieldQueryable("Position", position);
                Filter? filter = await query.FirstOrDefaultAsync();
                if (filter == null) { return null; }
                return filter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy filter tại position : {position} lỗi : {ex}");
                return null;
            }
        }

        public async Task<List<Filter>?> GetAllFilter()
        {
            try
            {
                IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetAll();
                List<Filter> list = await query.ToListAsync();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy danh sách filter : {ex}");
                return null;
            }
        }



        //public async Task<Filter?> GetFilterDetailByFilterID(int filterID)
        //{
        //    try
        //    {
        //        IQueryable<Filter> query = this.unitOfWork.FilterDetailRepository().GetByIdQueryable(filterID);
        //        Filter? filter = await query.FirstOrDefaultAsync();
        //        if (filter == null) { return null; }
        //        return filter;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi lấy filter id : {filterID} lỗi : {ex}");
        //        return null;
        //    }
        //}

        //public async Task<Filter?> GetFilterByPosition(string position)
        //{
        //    try
        //    {
        //        IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetByFieldQueryable("Position", position);
        //        Filter? filter = await query.FirstOrDefaultAsync();
        //        if (filter == null) { return null; }
        //        return filter;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi lấy filter tại position : {position} lỗi : {ex}");
        //        return null;
        //    }
        //}

        //public async Task<List<Filter>?> GetAllFilter()
        //{
        //    try
        //    {
        //        IQueryable<Filter> query = this.unitOfWork.FilterRepository().GetAll();
        //        List<Filter> list = await query.ToListAsync();
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi lấy danh sách filter : {ex}");
        //        return null;
        //    }
        //}
    }
}
