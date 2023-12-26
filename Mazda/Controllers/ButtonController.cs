using Mazda.Base;
using Mazda.Data.UnitofWork;
using Mazda.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazda.Controllers
{
    public class ButtonController : BaseController
    {
        public ButtonController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration) : base(unitofWork, dataContext, configuration)
        {
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await UnitofWork.Repository<Button>().GetAllAsync();
            return Ok(list);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Button button)
        {

            if (button is null)
            {
                return NotFound("not found");
            }
            await UnitofWork.Repository<Button>().AddAsync(button);
            var check = await UnitofWork.Complete();
            if (check > 0)
            {
                return Ok(new
                {
                    message = "Thành công"
                });
            }
            return BadRequest("Lỗi không thêm");
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(Button button)
        {
            try
            {
                var existingAboutUs = await UnitofWork.Repository<Button>().GetByIdAsync(button.Id);
                if (existingAboutUs == null)
                {
                    return NotFound("Không tìm thấy");
                }
                existingAboutUs.Name = button.Name;
                existingAboutUs.Link = button.Link;
                await UnitofWork.Repository<Button>().Update(existingAboutUs);
                var check = await UnitofWork.Complete();
                if (check > 0)
                {
                    return Ok(new
                    {
                        message = "Thành công"
                    });
                }
                return BadRequest("Lỗi không thêm");


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCategoryType = await UnitofWork.Repository<Button>().GetByIdAsync(id);
            if (existingCategoryType == null)
            {
                return Ok(new
                {
                    message = "Không tìm thấy"
                });
            }
            else
            {
                await UnitofWork.Repository<Button>().Delete(existingCategoryType);
                var check = await UnitofWork.Complete();
                if (check > 0)
                {
                    return Ok(new
                    {
                        message = "Thành công"
                    });
                }
            }
            return BadRequest("Lỗi không thêm");
        }
    }
}
