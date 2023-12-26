using Mazda.Model;

namespace Mazda.Dtos.Product
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public double? Price_After { get; set; }
        public string? Description { get; set; }
        public string? UrlShoppe { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string>? Paths { get; set; }
        public IFormFileCollection? FormCollection { get; set; }
        public IFormFileCollection? FormCollectionAvatar { get; set; }


    }
}
