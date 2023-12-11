using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Mazda.Helper
{
    public class ImageService
    {
        private readonly IWebHostEnvironment environment;
        private IHttpContextAccessor httpContextAccessor;
        public ImageService(IWebHostEnvironment environment, IHttpContextAccessor _httpContextAccessor)
        {

            this.environment = environment;
            this.httpContextAccessor = _httpContextAccessor;
        }
        private string GetFilepath(string productcode, string type)
        {

            return Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "Upload", type, productcode);
        }
        public async Task<List<string>> Upload_Image(string productcode, string type, IFormFileCollection fileCollection)
        {
            try
            {
                string Filepath = GetFilepath(productcode, type);
                if (!Directory.Exists(Filepath))
                {
                    Directory.CreateDirectory(Filepath);
                }
                
                
                var result = new List<string>();
                if(fileCollection != null)
                {
                    result = await ResizeImage(fileCollection, Filepath);
                }          
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ImageDto> GetUrlImage(string productcode, string type)
        {
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            var Imageurl = new List<ImageDto>();
            string hosturl = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";
            var path_img = new List<string>(); 
            try
            {
                string Filepath = GetFilepath(productcode, type);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath;


                        imagepath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagepath))
                        {
                            string _Imageurl = hosturl + "/Upload/" + type + "/" + productcode + "/" + filename;                      
                            Imageurl.Add(new ImageDto
                            {
                                Image = _Imageurl,
                                Path = Path.Combine("wwwroot", "Upload", type, productcode, filename),
                            });
                        }


                    }
                }
                return Imageurl;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<string> DeleteImage(List<string> path_images)
        {
            var result = new List<string>();
            if(path_images != null)
            {
                foreach (var path in path_images)
                {
                    var path_image = Path.Combine(Directory.GetCurrentDirectory(), path);

                    try
                    {
                        File.Delete(path_image);
                        result.Add($"Removed image {path} from the server successfully");
                    }
                    catch (Exception ex)
                    {
                        result.Add($"Failed to remove image {path}: {ex.Message}");
                    }
                }
            }
            
            return result;
        }
        public string DeleteImage(string productcode, string type)
        {
            try
            {        
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","Upload", type, productcode);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return "Xóa Thành Công";
                }
                return "Xóa ảnh chưa được thực thi";
            }catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<List<string>> Update_Image(string objectdto,string? object1,string type,List<string> paths,IFormFileCollection fileCollection)
        {
            List<string> result = new List<string>();

            if (paths != null || fileCollection != null)
            {
                string Filepath = GetFilepath(object1, type);
                string FilePath_objectdto = GetFilepath(objectdto, type);
                var image_path_fe = new List<string>();
                var image_path_sever = new List<string>();
                var differnce_image = new List<string>();
                var existing_path = new List<string>();
                //Update toàn bộ ảnh DONE
                if (paths == null)
                {
                    if (Directory.Exists(Filepath))
                    {
                        Directory.Delete(Filepath, true);
                    }
                    result = await Upload_Image(objectdto, type, fileCollection);
                }
                //Update 1 vài ảnh 
                else
                {
                    //Lấy đường dẫn FE truyền xuống và đưa vào trong mảng
                    foreach (var path in paths)
                    {
                        string cleanedData = path.Replace("\\\\", "\\");

                        string path_api = Path.Combine(Directory.GetCurrentDirectory(), cleanedData);

                        image_path_fe.Add(path_api);
                    }

                    string get_curren_folder;
                    string subFolder = String.Equals(objectdto, object1, StringComparison.OrdinalIgnoreCase) ? objectdto : object1;
                    get_curren_folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", type, subFolder);
                    var get_curren_folde_img = Directory.GetFiles(get_curren_folder);
                    //lấy ảnh trong hệ thống và đưa vào trong mảng
                    foreach (var image_sever in get_curren_folde_img)
                    {
                        image_path_sever.Add(image_sever);
                    }
                    //Trường hợp trùng tên
                    if (Directory.Exists(FilePath_objectdto))
                    {
                        //Tìm kiếm những image khác trong image_path_sever
                        differnce_image = image_path_sever.Intersect(image_path_fe).ToList();
                        if (differnce_image.Count > 0)
                        {
                            foreach (string element in differnce_image)
                            {
                                File.Delete(element);
                            }
                        }
                        if (fileCollection != null)
                        {
                            result = await Upload_Image(objectdto, type, fileCollection);
                        }
                    }
                    //khác tên productcode                
                    else
                    {
                        //tìm pathkhác nhau
                        differnce_image = image_path_sever.Intersect(image_path_fe).ToList();
                        if (differnce_image.Count > 0)
                        {
                            foreach (string element in differnce_image)
                            {
                                File.Delete(element);
                            }
                        }
                        //tìm path giống nhau 
                        existing_path = image_path_sever.Except(image_path_fe).ToList();
                        string des_Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", type, objectdto);
                        Directory.CreateDirectory(des_Path);
                        if (existing_path.Count > 0)
                        {
                            foreach (string element in existing_path)
                            {
                                string fileName = System.IO.Path.GetFileName(element);
                                string destinationPath = Path.Combine(des_Path, fileName);

                                File.Copy(element, destinationPath, true);
                            }
                        }
                        if (fileCollection != null)
                        {
                            result = await Upload_Image(objectdto, type, fileCollection);
                        }
                        /*
                         foreach (var file_image in fileCollection)
                         {
                             string destinationPath2 = Path.Combine(des_Path, Path.GetFileName(file_image.FileName));
                             using (FileStream stream = new FileStream(destinationPath2, FileMode.Append))
                             {
                                 file_image.CopyToAsync(stream);

                             }
                         }   
                        */
                        Directory.Delete(get_curren_folder, true);

                    }

                }
            }            
            else if(!objectdto.Equals(object1))
            {
                var get_curren_folder = GetFilepath(object1, type);
                var get_curren_folde_img = Directory.GetFiles(get_curren_folder);
                var get_objectdto_file = GetFilepath(objectdto, type);
                Directory.CreateDirectory(get_objectdto_file);
                foreach(var item in get_curren_folde_img)
                {
                    string fileName = System.IO.Path.GetFileName(item);
                    string destinationPath = Path.Combine(get_objectdto_file, fileName);

                    File.Copy(item, destinationPath, true);
                }
                if (Directory.Exists(get_curren_folder))
                {
                    Directory.Delete(get_curren_folder, true);
                }
            }
            return result;

        }
        public async Task<List<string>> ResizeImage(IFormFileCollection colllection,string path)
        {
            int width = 2000;
            int height = 2000;
            var newImage = new Bitmap(width, height);
            var result = new List<string>();                   
            foreach(var file in colllection)
            {
                
                //đường dẫn ảnh
                string imagepath = Path.Combine(path, file.FileName);
                if (File.Exists(imagepath))
                {
                    File.Delete(imagepath);
                }
                Image image = Image.FromStream(file.OpenReadStream(), true, true);
                    using (var a = Graphics.FromImage(newImage))
                    {
                        a.DrawImage(image, 0, 0, width, height);
                        newImage.Save(imagepath);
                        result.Add(imagepath);
                    }
            }
            return result;
        }
    }
    
    public class ImageDto
    {
        public string Image { get; set; }
        public string Path { get; set; }
    }

}

