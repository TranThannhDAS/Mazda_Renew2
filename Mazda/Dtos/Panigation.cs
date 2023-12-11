namespace Mazda_Api.Dtos
{
    public class Panigation
    {
        public int PageIndex { get; set; } = 1;

        private int pageSize = 10;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > 10 ? 10 : value; }
        }
        public string? Search_CategoryName { get; set; }
        public string? Search_Name { get; set; }
    }
}
