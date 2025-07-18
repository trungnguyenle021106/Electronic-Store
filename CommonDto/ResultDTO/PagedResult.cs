using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDto.ResultDTO
{
    public record PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Tổng số lượng mục dữ liệu trong toàn bộ tập hợp (trước khi phân trang).
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Số trang hiện tại (thường bắt đầu từ 1).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Số lượng mục dữ liệu trên mỗi trang.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số trang có thể có, được tính toán dựa trên TotalCount và PageSize.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Cho biết có trang trước đó không.
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Cho biết có trang tiếp theo không.
        /// </summary>
        public bool HasNextPage => Page < TotalPages;
    }
}
