using Mazda;
using Mazda.Base;
using Mazda.Data.UnitofWork;
using Mazda.Helper.Cache;
using Mazda.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.WebSockets;

namespace Mazda.Controllers
{
    public class CategoryTypeController : BaseController
    {
        public static Dictionary<int, string> DictinoryCategoryType = new Dictionary<int, string>();
        public static List<CategoryType> ListCategoryType = new List<CategoryType>();
        public ICache cache1;
        public CategoryTypeController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, ICache cache) : base(unitofWork, dataContext, configuration)
        {
            cache1 = cache;
        }
        [HttpGet]
        public  async Task<IActionResult> GetAll()
        {
            var list = await UnitofWork.Repository<CategoryType>().GetAllAsync();
            return Ok(list);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CategoryType categoryType)
        {

            if (categoryType is null)
            {
                return NotFound("Category not found");
            }
            var categorytype1 = new CategoryType();
            categorytype1.Id = categoryType.Id;
            categorytype1.Name = categoryType.Name;  
            await UnitofWork.Repository<CategoryType>().AddAsync(categorytype1);
            var check = await UnitofWork.Complete();
            if (check > 0)
            {
                //Xóa cache
                DictinoryCategoryType.Clear();
                return Ok(new
                {
                    message = "Thành công"
                });
            }
            return BadRequest("Lỗi không thêm");
        }
        [HttpPut]
        public async Task<IActionResult> Update(CategoryType categoryType)
        {
            var existingCategoryType = await UnitofWork.Repository<CategoryType>().GetByIdAsync(categoryType.Id);
            if (existingCategoryType == null)
            {
                return Ok(new
                {
                    message = "Không tìm thấy"
                });
            }
            else
            {
                existingCategoryType.Name = categoryType.Name;
                await UnitofWork.Repository<CategoryType>().Update(existingCategoryType);
                var check = await UnitofWork.Complete();
                if (check > 0)
                {
                    //Xóa cache
                    DictinoryCategoryType.Clear();
                    return Ok(new
                    {
                        message = "Thành công"
                    });
                }
            }
            return BadRequest("Lỗi không thêm");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCategoryType = await UnitofWork.Repository<CategoryType>().GetByIdAsync(id);
            if (existingCategoryType == null)
            {
                return Ok(new
                {
                    message = "Không tìm thấy"
                });
            }
            else
            {
                await UnitofWork.Repository<CategoryType>().Delete(existingCategoryType);
                var check = await UnitofWork.Complete();
                if (check > 0)
                {
                    //Xóa cache
                    DictinoryCategoryType.Clear();
                    return Ok(new
                    {
                        message = "Thành công"
                    });
                }
            }
            return BadRequest("Lỗi không thêm");
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async  Task<string> GetCategoryType(int categoryTypeId)
        {
            string value;
            if (DictinoryCategoryType.ContainsKey(categoryTypeId))
            {
                value = DictinoryCategoryType[categoryTypeId];
            }
            else
            {
                var list = await UnitofWork.Repository<CategoryType>().GetAllAsync();
                foreach (var item in list)
                {
                    DictinoryCategoryType[item.Id] = item.Name;
                }
                value = DictinoryCategoryType[categoryTypeId];
            }

            return value;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<int> GetCategoryId(string name)
        {
            if (DictinoryCategoryType.Count == 0)
            {
                var list = await UnitofWork.Repository<CategoryType>().GetAllAsync();
                foreach (var item in list)
                {
                    DictinoryCategoryType[item.Id] = item.Name;
                }
            }
            var result = DictinoryCategoryType.Where(pair => pair.Value.Equals(name)).ToList();
            int targetKey = 0;

            if (result.Equals(default(KeyValuePair<int, string>)))
            {
                Console.WriteLine("Không tìm thấy giá trị 'Three' trong Dictionary.");
            }
            else
            {
                foreach(var test in result)
                {
                    targetKey = test.Key;

                }
                Console.WriteLine($"Key of 'Three': {targetKey}");
            }
            return targetKey;
        }
    }
    
}
