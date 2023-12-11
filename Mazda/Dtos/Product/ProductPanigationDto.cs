using Mazda.Helper;

namespace Mazda.Dtos.Product
{
    public class ProductPanigationDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public double? Price_After{ get; set; }
        public string? Description { get; set; }
        public string? UrlShoppe { get; set; }
        public List<ImageDto> UrlImage { get; set; }
    }
}
