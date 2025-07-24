using Microsoft.AspNetCore.Mvc;

namespace ProductService.Domain.DTO.Request
{
    public record ProductQueryParameter
    {
        [FromQuery] public int Page { get; init; } = 1; 
        [FromQuery] public int PageSize { get; init; } = 10;
        [FromQuery] public string? SearchText { get; init; }
        [FromQuery] public string? ProductBrandName { get; init; }
        [FromQuery] public string? ProductTypeName { get; init; }
        [FromQuery] public string? ProductStatus { get; init; }
        [FromQuery] public string? ProductPropertyName { get; init; }
        [FromQuery] public string? ProductPropertyDescription { get; init; }
        [FromQuery] public string? ProductPropertyIds { get; init; }
        [FromQuery] public bool? IsIncrease { get; init; } = true; 
    }
}
