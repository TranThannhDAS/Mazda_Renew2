using AutoMapper;
using Azure.Core;
using Mazda.Base;
using Mazda.Data.AboutUs;
using Mazda.Data.UnitofWork;
using Mazda.Dtos;
using Mazda.Dtos.Product;
using Mazda.Helper;
using Mazda.Model;
using Mazda_Api.Controllers;
using Mazda_Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Net;


namespace Mazda.Controllers
{
    public class ProductController : BaseController
    {
        public IMapper mapper;
        public ImageService imageService;
        public CategoryController categoryController;
        public GetInfoAboutUs GetInfoAboutUs;
        public IHttpContextAccessor httpContextAccessor;
        public ProductController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, IMapper _mapper, ImageService _imageService, CategoryController categoryController, GetInfoAboutUs getInfoAboutUs, IHttpContextAccessor httpContextAccessor) : base(unitofWork, dataContext, configuration)
        {
            mapper = _mapper;
            imageService = _imageService;
            this.categoryController = categoryController;
            GetInfoAboutUs = getInfoAboutUs;
            this.httpContextAccessor = httpContextAccessor;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDto productDto)
        {
            var categoryId = await categoryController.GetCategoryId(productDto.CategoryName);
            var product = mapper.Map<ProductDto, Product>(productDto);
            product.CategoryId = categoryId;
            string code = imageService.GenerateRandomString();
            if (productDto.FormCollection is not null)
            {
                var images = await imageService.Upload_Image(code, productDto.FormCollection);
                product.code = code;
            }
            if(productDto.FormCollectionAvatar is not null)
            {
                var images = await imageService.Upload_Image(code+"avatar", productDto.FormCollectionAvatar);
            }
            await UnitofWork.Repository<Product>().AddAsync(product);
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
        public async Task<IActionResult> Update([FromForm] ProductUpdateDto productUpdateDto)
        {
            var product = mapper.Map<ProductUpdateDto, Product>(productUpdateDto);
            var existingProduct = await UnitofWork.Repository<Product>().GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return BadRequest("Không tìm thấy product");
            }
            //lấy CategoryName          
                var images = await imageService.Update_Image(existingProduct.code, productUpdateDto.Paths, productUpdateDto.FormCollection);          
            if(productUpdateDto.FormCollectionAvatar is not null)
            {
                var avatar = await imageService.Update_Image(existingProduct.code+ "avatar",null, productUpdateDto.FormCollectionAvatar);
            }
            existingProduct.Name = productUpdateDto.Name;
            existingProduct.Price = productUpdateDto.Price;
            existingProduct.UrlShoppe = productUpdateDto.UrlShoppe;
            existingProduct.Price_After = productUpdateDto.Price_After;
            existingProduct.CategoryId = productUpdateDto.CategoryId;
            existingProduct.Description = productUpdateDto.Description;
            await UnitofWork.Repository<Product>().Update(existingProduct);
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
        [Authorize]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProduct = await UnitofWork.Repository<Product>().GetByIdAsync(id);
            if (existingProduct == null)
            {
                return BadRequest("Không tìm thấy product");
            }
            var categoryName = await categoryController.GetCategoryType(existingProduct.CategoryId);
            var images = imageService.DeleteImage(existingProduct.code);
            var avatar = imageService.DeleteImage(existingProduct.code+"avatar");
            await UnitofWork.Repository<Product>().Delete(existingProduct);

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
            var existingProduct = await UnitofWork.Repository<Product>().GetByIdAsync(id);
            if (existingProduct == null)
            {
                return BadRequest("Không tìm thấy product");
            }
            var categoryName = await categoryController.GetCategoryType(existingProduct.CategoryId);
            var aboutUs = GetInfoAboutUs.GetMess_CallAboutUs();
            return Ok(new
            {
                Product = existingProduct,
                CategoryName = categoryName,
                Info_in_AboutUs = aboutUs,
                UrlImage = imageService.GetUrlImage(existingProduct.code),
                Avatar = imageService.GetUrlImage(existingProduct.code+"avatar")
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetPaginationProduct(Panigation panigation)
        {
            var result = new List<ProductPanigationDto>();
            int categoryId = 0;
            if (panigation.Search_CategoryName != null)
            {
                categoryId = await categoryController.GetCategoryId(panigation.Search_CategoryName);
            }
            var product = DataContext.Products
      .Where(p =>
          (categoryId == 0 || p.CategoryId == categoryId) && // Điều kiện về categoryId
          (string.IsNullOrEmpty(panigation.Search_Name) || p.Name.Contains(panigation.Search_Name))).AsQueryable(); // Điều kiện về tên
            var pagiantion_product = product
      .Skip((panigation.PageIndex - 1) * panigation.PageSize)
      .Take(panigation.PageSize)
      .AsQueryable();


            int count = await product.CountAsync();
            var data = await pagiantion_product.ToListAsync();
            foreach (var item in data)
            {
                string categoryName = await categoryController.GetCategoryType(item.CategoryId);
                var data_product = new ProductPanigationDto
                {
                    ID = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price_After = item.Price_After,
                    Price = item.Price,
                    UrlShoppe = item.UrlShoppe,
                    UrlImage = imageService.GetUrlImage(item.code+"avatar")
                };
                result.Add(data_product);
            }
            int totalPageIndex =
            count % panigation.PageSize == 0
                ? count / panigation.PageSize
                : (count / panigation.PageSize) + 1;
            var panigationData = new PanigationGeneric<ProductPanigationDto>(panigation.PageIndex, panigation.PageSize, result, count, totalPageIndex);
            return Ok(panigationData);

        }
        //Fix lỗi
        [HttpGet]
        public async Task<IActionResult> ProductHasSeen(string id)
        {
            string ip = Response.HttpContext.Connection.RemoteIpAddress.ToString();
            if (ip == "::1")
            {
                ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[2].ToString();
            }
         
            return Ok(new
            {
                ip = ip,
                id = id,
            });
        } 

    }
}
