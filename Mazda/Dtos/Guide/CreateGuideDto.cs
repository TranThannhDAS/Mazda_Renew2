using Mazda.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazda.Dtos.Guide
{
    public class CreateGuideDto
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public string? UrlYoutube { get; set; }
        public string CategoryName { get; set; }
        public IFormFileCollection? FormCollection { get; set; }
    }
}
