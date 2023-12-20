using Mazda;
using Mazda.Data.UnitofWork;
using Mazda.Model;
using Mazda.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Mazda_Api.Controllers
{
 
    public class AboutUsController : BaseController
    {
        public AboutUsController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration) : base(unitofWork, dataContext, configuration)
        {
        }
        [HttpGet]
        public  async Task<IActionResult> GetAll()
        {
            var list = await UnitofWork.Repository<AboutUS>().GetAllAsync();
            return Ok(list);
        }
        [Authorize]
        [HttpPost]
        public  async Task<IActionResult> Create(AboutUS aboutUS)
        {

            if (aboutUS is null)
            {
                return NotFound("Cattegory not found");
            }
            await UnitofWork.Repository<AboutUS>().AddAsync(aboutUS);
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
        public  async Task<IActionResult> Update(AboutUS aboutUS)
        {
            try
            {
                var existingAboutUs = await UnitofWork.Repository<AboutUS>().GetByIdAsync(aboutUS.ID);
                if (existingAboutUs == null)
                {
                    return NotFound("Không tìm thấy");
                }
                existingAboutUs.Name = aboutUS.Name;
                existingAboutUs.google_map = aboutUS.google_map;
                existingAboutUs.Address = aboutUS.Address;
                existingAboutUs.Url_Mess = aboutUS.Url_Mess;
                existingAboutUs.Phone = aboutUS.Phone;
                existingAboutUs.Email = aboutUS.Email;
                await UnitofWork.Repository<AboutUS>().Update(existingAboutUs);
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
    
    }
}
