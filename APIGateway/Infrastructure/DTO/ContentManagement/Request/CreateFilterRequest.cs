namespace APIGateway.Infrastructure.DTO.ContentManagement.Request
{
    public record CreateFilterRequest
    (Filter? Filter, List<int>? productPropertyIDs);
}
