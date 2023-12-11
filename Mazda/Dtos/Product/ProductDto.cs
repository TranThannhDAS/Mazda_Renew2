using Mazda.Model;

namespace Mazda.Dtos.Product
{
    public class ProductDto
    {
        public string Name{ get; set; }
        public double? Price { get; set; }
        public int? Discount { get; set; }
        public string? Description { get; set; }
        public string? UrlShoppe { get; set; }
        public string? UrlYoutube { get; set; }
        public string CategoryName { get; set; }
        public IFormFileCollection? FormCollection { get; set; }
    }
}
