using Mazda.Helper;

namespace Mazda.Dtos.Guide
{
    public class GuidePanigationDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string? Content { get; set; }
        public string CategoryName { get; set; }
        public List<ImageDto> UrlImage { get; set; }
    }
}
