using AutoMapper;
using Mazda.Base;
using Mazda.Data.AboutUs;
using Mazda.Data.UnitofWork;
using Mazda.Dtos.Blog;
using Mazda.Dtos;
using Mazda.Helper;
using Mazda.Model;
using Mazda_Api.Controllers;
using Mazda_Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mazda.Dtos.Guide;

namespace Mazda.Controllers
{
    public class GuideController : BaseController
    {
        public IMapper mapper;
        public ImageService imageService;
        public CategoryController categoryController;
        public GetInfoAboutUs GetInfoAboutUs;
        public GuideController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, IMapper _mapper, ImageService _imageService, CategoryController categoryController, GetInfoAboutUs getInfoAboutUs) : base(unitofWork, dataContext, configuration)
        {
            mapper = _mapper;
            imageService = _imageService;
            this.categoryController = categoryController;
            GetInfoAboutUs = getInfoAboutUs;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateGuideDto createGuideDto)
        {
            var categoryId = await categoryController.GetCategoryId(createGuideDto.CategoryName);
            var guide = mapper.Map<CreateGuideDto, Guide>(createGuideDto);
            guide.CategoryId = categoryId;
            var images = await imageService.Upload_Image(createGuideDto.Name, createGuideDto.CategoryName, createGuideDto.FormCollection);
            await UnitofWork.Repository<Guide>().AddAsync(guide);
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
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateGuideDto updateGuideDto)
        {
            var guide = mapper.Map<UpdateGuideDto, Guide>(updateGuideDto);
            var existingGuide = await UnitofWork.Repository<Blog>().GetByIdAsync(guide.Id);
            if (existingGuide == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            var images = await imageService.Update_Image(updateGuideDto.Name, existingGuide.Name, updateGuideDto.CategoryName, updateGuideDto.Paths, updateGuideDto.FormCollection);
            existingGuide.Name = updateGuideDto.Name;
            existingGuide.Content = updateGuideDto.Content;
            existingGuide.CategoryId = updateGuideDto.CategoryId;
            await UnitofWork.Repository<Blog>().Update(existingGuide);
            int check = await UnitofWork.Complete();
            if (check > 0)
            {
                return Ok(new
                {
                    message = "Thành công"
                });
            }
            return BadRequest("Lỗi không thêm");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingGuide = await UnitofWork.Repository<Guide>().GetByIdAsync(id);
            if (existingGuide == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            var categoryName = await categoryController.GetCategoryType(existingGuide.CategoryId);
            var images = imageService.DeleteImage(existingGuide.Name, categoryName);
            await UnitofWork.Repository<Guide>().Delete(existingGuide);

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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByID(int id)
        {
            var existingGuide = await UnitofWork.Repository<Guide>().GetByIdAsync(id);
            if (existingGuide == null)
            {
                return BadRequest("Không tìm thấy product");
            }
            var categoryName = await categoryController.GetCategoryType(existingGuide.CategoryId);
            return Ok(new
            {
                Guide = existingGuide,
                CategoryName = categoryName,
                UrlImage = imageService.GetUrlImage(existingGuide.Name, categoryName)
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetPaginationProduct(Panigation panigation)
        {
            var result = new List<GuidePanigationDto>();
            int categoryId = 0;
            if (panigation.Search_CategoryName != null)
            {
                categoryId = await categoryController.GetCategoryId(panigation.Search_CategoryName);
            }
            var guide = DataContext.Guides.Where(p =>
          (categoryId == 0 || p.CategoryId == categoryId) && // Điều kiện về categoryId
          (string.IsNullOrEmpty(panigation.Search_Name) || p.Name.Contains(panigation.Search_Name))).AsQueryable(); // Điều kiện về tên
          var pagination_guide = guide
      .Skip((panigation.PageIndex - 1) * panigation.PageSize)
      .Take(panigation.PageSize)
      .AsQueryable();

            int count = await DataContext.Guides.CountAsync();
            var data = await pagination_guide.ToListAsync();
            foreach (var item in data)
            {
                string categoryName = await categoryController.GetCategoryType(item.CategoryId);
                var data_product = new GuidePanigationDto
                {
                    ID = item.Id,
                    Name = item.Name,
                    CategoryName = categoryName,
                    Content = item.Content,
                    UrlImage = imageService.GetUrlImage(item.Name, categoryName)
                };
                result.Add(data_product);
            }
            int totalPageIndex =
            count % panigation.PageSize == 0
                ? count / panigation.PageSize
                : (count / panigation.PageSize) + 1;
            var panigationData = new PanigationGeneric<GuidePanigationDto>(panigation.PageIndex, panigation.PageSize, result, count, totalPageIndex);
            return Ok(panigationData);

        }
  
    }
}
