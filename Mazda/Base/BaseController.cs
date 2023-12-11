using Mazda;
using Mazda.Data.UnitofWork;
using Microsoft.AspNetCore.Mvc;

namespace Mazda.Base
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public abstract class BaseController: ControllerBase
    {
        public IUnitofWork UnitofWork { get; set; }
        public DataContext DataContext { get; set; }
        public IConfiguration Configuration { get; set; }   
        public BaseController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration)
        {
            UnitofWork = unitofWork;
            DataContext = dataContext;
            Configuration = configuration;
        }
       
    }
}
