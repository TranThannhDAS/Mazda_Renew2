namespace Mazda.Model
{
    public class ProductsViewed
    {
        public int Id { get; set; } 
        public string Ip {  get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
