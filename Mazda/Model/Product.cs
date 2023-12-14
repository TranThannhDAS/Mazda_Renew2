using System.ComponentModel.DataAnnotations.Schema;

namespace Mazda.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? Price {  get; set; }
        public double? Price_After { get; set; }
        [Column(TypeName = "ntext")]
        public string? Description { get; set; }
        public string? UrlShoppe { get; set; }
        public string? code { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

    }
}
