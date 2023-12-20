using Mazda;
using Mazda.Data.UnitofWork;
using Mazda.Model;
using Mazda.Base;
using Mazda_Api.Dtos.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mazda.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Mazda_Api.Controllers
{
    //Fix lỗi khi Create và Update
    public class CategoryController : BaseController
    {
        public CategoryTypeController CategoryTypeController;
        public static Dictionary<int, string> DictinoryCategory = new Dictionary<int, string>();

        public CategoryController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, CategoryTypeController categoryTypeController) : base(unitofWork, dataContext, configuration)
        {
            CategoryTypeController = categoryTypeController;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (category is null)
            {
                return NotFound("Cattegory not found");
            }
            await UnitofWork.Repository<Category>().AddAsync(category);
            var check = await UnitofWork.Complete();
            if (check > 0)
            {
                DictinoryCategory.Clear();
                return Ok(new
                {
                    message = "Thành công"
                });
            }
            return BadRequest("Lỗi không thêm");
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(Category category)
        {
            var exsistingCategory =  await UnitofWork.Repository<Category>().GetByIdAsync(category.Id);
            if (exsistingCategory is null)
            {
                return NotFound("Cattegory not found");
            }
            exsistingCategory.Name = category.Name;
            exsistingCategory.CategoryTypeId = category.CategoryTypeId;
            var check = await UnitofWork.Complete();
            if (check > 0)
            {
                DictinoryCategory.Clear();
                return Ok(new
                {
                    message = "Thành công"
                });
            }
            return BadRequest("Lỗi không thêm");
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exsistingCategory = await UnitofWork.Repository<Category>().GetByIdAsync(id);
            if (exsistingCategory == null)
            {
                return NotFound("Không tìm thấy");
            }
            var product = DataContext.Products.Where(p => p.CategoryId == exsistingCategory.Id).ToList();
            var blog = DataContext.Blog.Where(p => p.CategoryId == exsistingCategory.Id).ToList();
            var guide = DataContext.Guides.Where(p => p.CategoryId == exsistingCategory.Id).ToList();

            if (product != null)
            {
                await UnitofWork.Repository<Product>().DeleteRange(product);
               int check = await UnitofWork.Complete();
            }
            if (blog != null)
            {
                await UnitofWork.Repository<Blog>().DeleteRange(blog);
                int check = await UnitofWork.Complete();
            }
            if (guide != null)
            {
                await UnitofWork.Repository<Guide>().DeleteRange(guide);
                int check = await UnitofWork.Complete();
            }
            await UnitofWork.Repository<Category>().Delete(exsistingCategory);
            int check1 = await UnitofWork.Complete();
            if(check1 > 0)
            {
                DictinoryCategory.Clear();
                return Ok(new
                {
                    mess = "Xóa thành công"
                });
            }
            return BadRequest();
        }
        [HttpGet("{categoryType}")]
        public async Task<IActionResult> GetAll(string categoryType)
        {
            var categoryTypeId = await CategoryTypeController.GetCategoryId(categoryType);
            var categoryDTOList = new List<CategoryDto>();
            var categoryList = DataContext.Categories.Where( p => p.CategoryTypeId == categoryTypeId ).ToList();         
            foreach (var category in categoryList)
            {
                var categoryTypeName1 = await CategoryTypeController.GetCategoryType(category.CategoryTypeId);
                var categoryDTO = new CategoryDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CategoryTypeId = category.CategoryTypeId,
                    CategoryTypeName = categoryTypeName1
                };

                categoryDTOList.Add(categoryDTO);
            }

            return Ok(categoryDTOList);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<string> GetCategoryType(int categoryId)
        {
            string value;
            if (DictinoryCategory.ContainsKey(categoryId))
            {
                value = DictinoryCategory[categoryId];
            }
            else
            {
                var list = await UnitofWork.Repository<Category>().GetAllAsync();
                foreach (var item in list)
                {
                    DictinoryCategory[item.Id] = item.Name;
                }
                value = DictinoryCategory[categoryId];
            }
     
            return value;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<int> GetCategoryId(string name)
        {
            if (DictinoryCategory.Count == 0)
            {
                var list = await UnitofWork.Repository<Category>().GetAllAsync();
                foreach (var item in list)
                {
                    DictinoryCategory[item.Id] = item.Name;
                }
            }
            var result = DictinoryCategory.FirstOrDefault(pair => pair.Value.Equals(name));
            int targetKey = 0;

            if (result.Equals(default(KeyValuePair<int, string>)))
            {
                Console.WriteLine("Không tìm thấy giá trị 'Three' trong Dictionary.");
            }
            else
            {
                targetKey = result.Key;
                Console.WriteLine($"Key of 'Three': {targetKey}");
            }
            return targetKey;
        }
    }
}
