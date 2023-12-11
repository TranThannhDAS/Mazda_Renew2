using Mazda.Model;

namespace Mazda.Dtos.Blog
{
    public class UpdateBlogDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Content { get; set; }
        public string? UrlYoutube { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<string>? Paths { get; set; }
        public IFormFileCollection? FormCollection { get; set; }
    }
}
