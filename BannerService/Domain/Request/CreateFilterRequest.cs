using ContentManagementService.Domain.Entities;

namespace ContentManagementService.Domain.Request
{
    public record CreateFilterRequest
    (Filter Filter, List<int> productPropertyIDs);
}
