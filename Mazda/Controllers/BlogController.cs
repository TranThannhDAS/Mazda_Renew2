using AutoMapper;
using Mazda.Base;
using Mazda.Data.AboutUs;
using Mazda.Data.UnitofWork;
using Mazda.Dtos.Product;
using Mazda.Dtos;
using Mazda.Helper;
using Mazda.Model;
using Mazda_Api.Controllers;
using Mazda_Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mazda.Dtos.Blog;

namespace Mazda.Controllers
{
    public class BlogController : BaseController
    {
        public IMapper mapper;
        public ImageService imageService;
        public CategoryController categoryController;
        public GetInfoAboutUs GetInfoAboutUs;
        public BlogController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, IMapper _mapper, ImageService _imageService, CategoryController categoryController, GetInfoAboutUs getInfoAboutUs) : base(unitofWork, dataContext, configuration)
        {
            mapper = _mapper;
            imageService = _imageService;
            this.categoryController = categoryController;
            GetInfoAboutUs = getInfoAboutUs;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBlogDto createBlogDto)
        {
            var categoryId = await categoryController.GetCategoryId(createBlogDto.CategoryName);
            var blog = mapper.Map<CreateBlogDto, Blog>(createBlogDto);
            blog.CategoryId = categoryId;
            var images = await imageService.Upload_Image(createBlogDto.Name, createBlogDto.CategoryName, createBlogDto.FormCollection);
            await UnitofWork.Repository<Blog>().AddAsync(blog);
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
        public async Task<IActionResult> Update([FromForm] UpdateBlogDto updateBlogDto)
        {
            var blog = mapper.Map<UpdateBlogDto, Blog>(updateBlogDto);
            var existingBlog = await UnitofWork.Repository<Blog>().GetByIdAsync(blog.Id);
            if (existingBlog == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            var images = await imageService.Update_Image(updateBlogDto.Name, existingBlog.Name, updateBlogDto.CategoryName, updateBlogDto.Paths, updateBlogDto.FormCollection);
            existingBlog.Name = updateBlogDto.Name;
            existingBlog.Content = updateBlogDto.Content;
            existingBlog.CategoryId = updateBlogDto.CategoryId;
            await UnitofWork.Repository<Blog>().Update(existingBlog);
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
            var existingBlog = await UnitofWork.Repository <Blog>().GetByIdAsync(id);
            if (existingBlog == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            var categoryName = await categoryController.GetCategoryType(existingBlog.CategoryId);
            var images = imageService.DeleteImage(existingBlog.Name, categoryName);
            await UnitofWork.Repository<Blog>().Delete(existingBlog);

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
            var existingBlog = await UnitofWork.Repository<Blog>().GetByIdAsync(id);
            if (existingBlog == null)
            {
                return BadRequest("Không tìm thấy product");
            }
            var categoryName = await categoryController.GetCategoryType(existingBlog.CategoryId);
            return Ok(new
            {
                Blog = existingBlog,
                CategoryName = categoryName,
                UrlImage = imageService.GetUrlImage(existingBlog.Name, categoryName)
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetPaginationProduct(Panigation panigation)
        {
            var result = new List<BlogPanigationDto>();
            int categoryId = 0;
            if (panigation.Search_CategoryName != null)
            {
                categoryId = await categoryController.GetCategoryId(panigation.Search_CategoryName);
            }
            var product = DataContext.Blog.Where(p =>
          (categoryId == 0 || p.CategoryId == categoryId) && // Điều kiện về categoryId
          (string.IsNullOrEmpty(panigation.Search_Name) || p.Name.Contains(panigation.Search_Name))).AsQueryable(); // Điều kiện về tên
            var pagination_product = product
      .Skip((panigation.PageIndex - 1) * panigation.PageSize)
      .Take(panigation.PageSize)
      .AsQueryable();

            int count = await DataContext.Blog.CountAsync();
            var data = await product.ToListAsync();
            foreach (var item in data)
            {
                string categoryName = await categoryController.GetCategoryType(item.CategoryId);
                var data_product = new BlogPanigationDto
                {
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
            var panigationData = new PanigationGeneric<BlogPanigationDto>(panigation.PageIndex, panigation.PageSize, result, count, totalPageIndex);
            return Ok(panigationData);

        }
    }
}
