using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mazda_Api.Dtos.Category
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryTypeId { get; set; }
        public string CategoryTypeName { get; set; }
    }
}
