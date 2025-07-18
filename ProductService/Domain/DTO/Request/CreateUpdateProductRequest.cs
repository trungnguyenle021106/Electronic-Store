using ProductService.Domain.Entities;

namespace ProductService.Domain.DTO.Request
{
    public class CreateUpdateProductRequest
    {
        public string Name { get; set; } // Khớp với Product.Name
        public int ProductTypeID { get; set; }
        public int Quantity { get; set; } // Khớp với Product.Quantity
        public int ProductBrandID { get; set; } // Khớp với Product.Brand
        public string Description { get; set; } // Khớp với Product.Description
        public float Price { get; set; } // Khớp với Product.Price
        public string Status { get; set; } // Khớp với Product.Status
        public string? Image { get; set; } // Khớp với Product.Status
        // Danh sách ProductPropertyIDs (khớp với formData.append('ProductPropertyIDs', ...))
        public List<int>? ProductPropertyIDs { get; set; }

        // File (khớp với formData.append('File', ...))
        public IFormFile? File { get; set; } // Nếu chỉ có một file
    }
}
