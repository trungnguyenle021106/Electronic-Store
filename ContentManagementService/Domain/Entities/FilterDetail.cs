using ContentManagementService.Domain.Entities;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace ContentManagementService.Domain.Entities
{
    public class FilterDetail
    {
        public required int FilterID { get; set; }
        public required int ProductPropertyID { get; set; }

        [JsonIgnore]
        public Filter Filter { get; set; } = null!;
    }
}
