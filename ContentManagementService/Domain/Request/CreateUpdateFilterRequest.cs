using ContentManagementService.Domain.Entities;

namespace ContentManagementService.Domain.Request
{
    public record CreateUpdateFilterRequest

    (Filter Filter, List<int> productPropertyIDs);
}
