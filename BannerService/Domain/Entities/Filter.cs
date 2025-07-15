using ContentManagementService.Domain.Entities;
using Microsoft.Extensions.Hosting;

namespace ContentManagementService.Domain.Entities
{
    public class Filter
    {
        public int ID { get; set; }
        public required string Position { get; set; } = "";

        public ICollection<FilterDetail> FilterDetails { get; } = new List<FilterDetail>();
    }
}
