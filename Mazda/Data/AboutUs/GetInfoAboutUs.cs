using System.Linq;
using System.Numerics;

namespace Mazda.Data.AboutUs
{
    public class GetInfoAboutUs
    {
        public DataContext Context { get; set; }
        public GetInfoAboutUs(DataContext dataContext)
        {
            Context = dataContext;
        }
        public List<AboutUsInfo> GetMess_CallAboutUs()
        {
            var query = Context.AboutUs.Select(
            
              per => new AboutUsInfo
              {
                 Url_Mess = per.Url_Mess,
                  Phone = per.Phone
              }
            );
            var result = query.ToList();
            return result;
        }
    }
    public class AboutUsInfo
    {
        public string Url_Mess { get; set; }
        public string Phone { get; set; }
    }
}
