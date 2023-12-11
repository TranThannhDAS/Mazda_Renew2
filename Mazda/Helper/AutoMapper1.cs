using AutoMapper;
using Mazda.Dtos.Blog;
using Mazda.Dtos.Guide;
using Mazda.Dtos.Product;
using Mazda.Model;

namespace backend.Helper
{
    public class AutoMapper1 : Profile
    {
        public AutoMapper1()
        {
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<ProductUpdateDto, Product>().ReverseMap();
            CreateMap<CreateBlogDto, Blog>().ReverseMap();
            CreateMap<UpdateBlogDto, Blog>().ReverseMap();
            CreateMap<CreateGuideDto, Guide>().ReverseMap();
            CreateMap<UpdateGuideDto, Guide>().ReverseMap();

        }
    }
}
