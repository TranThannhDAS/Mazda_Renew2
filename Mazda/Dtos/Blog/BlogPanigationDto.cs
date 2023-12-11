using Mazda.Helper;

namespace Mazda.Dtos.Blog
{
    public class BlogPanigationDto
    {
        public string Name { get; set; }
        public string? Content { get; set; }
        public string CategoryName { get; set; }
        public List<ImageDto> UrlImage { get; set; }
    }
}
