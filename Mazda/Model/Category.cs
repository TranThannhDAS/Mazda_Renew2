namespace Mazda.Model
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int CategoryTypeId { get; set; }
        public CategoryType? CategoryType { get; set; }

    }
}
