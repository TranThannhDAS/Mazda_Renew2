using System.ComponentModel.DataAnnotations.Schema;

namespace Mazda.Model
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Create_Date { get; set; } = DateTime.Now;
        [Column(TypeName = "ntext")]
        public string? Content { get; set; }
        public string? code { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
