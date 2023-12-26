using System;
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
        private string GetFilepath(string random)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", random);
        }
        public async Task<List<string>> Upload_Image(string random, IFormFileCollection fileCollection)
        {
            try
            {
                string Filepath = GetFilepath(random);
                try
                {
                    if (!Directory.Exists(Filepath))
                    {
                        Directory.CreateDirectory(Filepath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                }
                var result = new List<string>();
                if (fileCollection != null)
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
        static void CheckDirectoryAccess(string directoryPath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                if (directoryInfo.Exists)
                {
                    // Kiểm tra quyền truy cập đọc
                    if (directoryInfo.Exists && (directoryInfo.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        Console.WriteLine("Read-only access to the directory.");
                    }
                    else
                    {
                        Console.WriteLine("Read-write access to the directory.");
                    }

                    // Kiểm tra quyền truy cập thực thi
                    if ((directoryInfo.Attributes & FileAttributes.Directory) != 0)
                    {
                        Console.WriteLine("Executable (can enter) access to the directory.");
                    }
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking directory access: {ex.Message}");
            }
        }

        public List<ImageDto> GetUrlImage(string code_random)
        {
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            var Imageurl = new List<ImageDto>();
            string hosturl = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";
            var path_img = new List<string>();
            try
            {
                string Filepath = GetFilepath(code_random);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagepath;


                        imagepath = Path.Combine(Filepath,filename);
                        if (System.IO.File.Exists(imagepath))
                        {
                            string _Imageurl = hosturl + "/Upload/" + code_random + "/" + filename;
                            Imageurl.Add(new ImageDto
                            {
                                Image = _Imageurl,
                                Path = Path.Combine("wwwroot", "Upload", code_random, filename),
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
            if (path_images != null)
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
        public string DeleteImage(string code_random)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", code_random);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return "Xóa Thành Công";
                }
                return "Xóa ảnh chưa được thực thi";
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<List<string>> Update_Image(string code, List<string>? paths, IFormFileCollection fileCollection)
        {
            List<string> result = new List<string>();

            if (paths != null || fileCollection != null)
            {
                string Filepath = GetFilepath(code);
                var image_path_fe = new List<string>();
                var image_path_sever = new List<string>();
                var diff_image = new List<string>();
                var existing_path = new List<string>();
                //Update toàn bộ ảnh DONE
                if (paths == null)
                {
                    if (Directory.Exists(Filepath)) 
                    {
                        Directory.Delete(Filepath, true);
                    }
                    result = await Upload_Image(code, fileCollection);
                }
                //Update 1 vài ảnh 
                else
                {
                    //Lấy đường dẫn FE truyền xuống và đưa vào trong mảng
                    string cleanedData = "";
                    foreach (var path in paths)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                             cleanedData = path.Replace("////", "//");// Fix cho Linux
                        }
                        else
                        {
                             cleanedData = path.Replace("\\\\", "\\");
                        }

                        string path_api = Path.Combine(Directory.GetCurrentDirectory(), cleanedData);

                        image_path_fe.Add(path_api);
                    }

                    string get_curren_folder;
                    get_curren_folder = GetFilepath(code);
                    var get_curren_folde_img = Directory.GetFiles(get_curren_folder);
                    //lấy ảnh trong hệ thống và đưa vào trong mảng
                    foreach (var image_sever in get_curren_folde_img)
                    {
                        image_path_sever.Add(image_sever);
                    }
                    //tìm ảnh khác nhau và xóa nó
                    diff_image = image_path_sever.Except(image_path_fe).ToList();
                    if (diff_image.Count > 0)
                    {
                        foreach (string element in diff_image)
                        {
                            File.Delete(element);
                        }
                    }
                    if (fileCollection != null)
                    {
                        result = await Upload_Image(code, fileCollection);
                    }
                    //        //Trường hợp trùng tên
                    //        if (Directory.Exists(FilePath_objectdto))
                    //        {
                    //            //Tìm kiếm những image khác trong image_path_sever
                    //            differnce_image = image_path_sever.Intersect(image_path_fe).ToList();
                    //            if (differnce_image.Count > 0)
                    //            {
                    //                foreach (string element in differnce_image)
                    //                {
                    //                    File.Delete(element);
                    //                }
                    //            }
                    //            if (fileCollection != null)
                    //            {
                    //                result = await Upload_Image(objectdto, type, fileCollection);
                    //            }
                    //        }
                    //        //khác tên productcode                
                    //        else
                    //        {
                    //            //tìm pathkhác nhau
                    //            differnce_image = image_path_sever.Intersect(image_path_fe).ToList();
                    //            if (differnce_image.Count > 0)
                    //            {
                    //                foreach (string element in differnce_image)
                    //                {
                    //                    File.Delete(element);
                    //                }
                    //            }
                    //            //tìm path giống nhau 
                    //            existing_path = image_path_sever.Except(image_path_fe).ToList();
                    //            string des_Path = GetFilepath(code);
                    //            Directory.CreateDirectory(des_Path);
                    //            if (existing_path.Count > 0)
                    //            {
                    //                foreach (string element in existing_path)
                    //                {
                    //                    string fileName = System.IO.Path.GetFileName(element);
                    //                    string destinationPath = Path.Combine(des_Path, fileName);

                    //                    File.Copy(element, destinationPath, true);
                    //                }
                    //            }
                    //            if (fileCollection != null)
                    //            {
                    //                result = await Upload_Image(objectdto, type, fileCollection);
                    //            }
                    //            /*
                    //             foreach (var file_image in fileCollection)
                    //             {
                    //                 string destinationPath2 = Path.Combine(des_Path, Path.GetFileName(file_image.FileName));
                    //                 using (FileStream stream = new FileStream(destinationPath2, FileMode.Append))
                    //                 {
                    //                     file_image.CopyToAsync(stream);

                    //                 }
                    //             }   
                    //            */
                    //            Directory.Delete(get_curren_folder, true);

                    //        }

                    //    }
                    //}            
                    //else if(!objectdto.Equals(object1))
                    //{
                    //    var get_curren_folder = GetFilepath(object1, type);
                    //    var get_curren_folde_img = Directory.GetFiles(get_curren_folder);
                    //    var get_objectdto_file = GetFilepath(objectdto, type);
                    //    Directory.CreateDirectory(get_objectdto_file);
                    //    foreach(var item in get_curren_folde_img)
                    //    {
                    //        string fileName = System.IO.Path.GetFileName(item);
                    //        string destinationPath = Path.Combine(get_objectdto_file, fileName);

                    //        File.Copy(item, destinationPath, true);
                    //    }
                    //    if (Directory.Exists(get_curren_folder))
                    //    {
                    //        Directory.Delete(get_curren_folder, true);
                    //    }
                    //}

                }

            }
            return result;
        }
        public async Task<List<string>> ResizeImage(IFormFileCollection colllection, string path)
        {
            int width = 1000;
            int height = 1000;
            var result = new List<string>();
            foreach (var file in colllection)
            {
                //đường dẫn ảnh
                string imagepath = Path.Combine(path, file.FileName);
                if (File.Exists(imagepath))
                {
                    File.Delete(imagepath);
                }
                using(FileStream stream = System.IO.File.Create(imagepath))
                {
                    await file.CopyToAsync(stream);
                }
                //var img = Image.FromFile(imagepath);
                //var scaleImage = ImageResize.Scale(img, width, height);
                //scaleImage.SaveAs(path+ "\\abc.jpg");
                ////Image image = Image.FromStream(file.OpenReadStream(), true, true);
                ////using (var a = Graphics.FromImage(newImage))
                ////{
                ////    a.DrawImage(image, 0, 0, width, height);
                ////    newImage.Save(imagepath);
                ////    result.Add(imagepath);
                ////}
                //img.Dispose();
                //scaleImage.Dispose();

            }
            return result;
        }
        public string GenerateRandomString(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            string randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
        //public String Update_Avatar(String code, IFormCollection collection)
        //{
        //    string Filepath = GetFilepath(code);
        //    if(Directory.Exists(Filepath))
        //    {

        //    }
        //}
    }

    public class ImageDto
    {
        public string Image { get; set; }
        public string Path { get; set; }
    }
}
