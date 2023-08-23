using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartAdmin.WebUI.ViewModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartAdmin.WebUI.Static
{
    public static class Photo
    {
        public static bool Delete(string webRootPath, string path)
        {
            if (!string.IsNullOrWhiteSpace(webRootPath) && !string.IsNullOrWhiteSpace(path))
            {
                var filePath34 =  Path.Combine(webRootPath, path);
                
                if (File.Exists(filePath34))
                {
                    File.Delete(filePath34);
                    return true;
                }                
            }
            return false;
        }
        public static async Task<string> UploadAsync(IFormFile File,string folderName)
        {
            if (File != null && File.Length > 0)
            {
                var fileName = Path.GetFileName(File.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    // The file already exists, so generate a new name
                    var extension = Path.GetExtension(fileName);
                    var newName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{extension}";
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName, newName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                return $"{folderName}/{Path.GetFileName(filePath)}";
            }
            return null;
        }
    }
}
