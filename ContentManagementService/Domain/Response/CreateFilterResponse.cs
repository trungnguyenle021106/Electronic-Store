using ContentManagementService.Domain.Entities;

namespace ContentManagementService.Domain.Response
{
    public record CreateFilterResponse
    (Filter Filter, List<int> productPropertyIDs);
}
