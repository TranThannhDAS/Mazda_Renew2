namespace Mazda.Dtos.Blog
{
    public class CreateBlogDto
    {
        public string Name { get; set; }
        public string? Content { get; set; }
        public string? UrlYoutube { get; set; }
        public string CategoryName { get; set; }
        public IFormFileCollection? FormCollection { get; set; }

    }
}
