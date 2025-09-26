using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Core.Utilities
{
    public static class AddImageHelper
    {
        public static StringResult chickImagePath(IFormFile file, string storagePath)
        {
            if (file is null)
            {
                return new StringResult();
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var contentType = file.ContentType.ToLower();
            if (!allowedExtensions.Contains(fileExtension) || !contentType.StartsWith("image/"))
            {
                return new StringResult { Message = "Invalid file type, Only images are allowed" };
            }
            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(storagePath, fileName);
            return new StringResult { Id = filePath };
        }
        public static async Task AddFile(IFormFile file, string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
        }
        public static async Task<string> BackupFiles(string path, string backupDir)
        {
            string backupFilesPath = "";
            if (File.Exists(path))
            {
                var backupPath = Path.Combine(backupDir, Path.GetFileName(path));
                File.Move(path, backupPath);
                backupFilesPath = backupPath;
            }
            return backupFilesPath;
        }
        public static void DeleteFiles(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        public static async Task RestoreBackupFiles(string storagePath, string backupDir)
        {
            foreach (var backupPath in Directory.GetFiles(backupDir))
            {
                var originalPath = Path.Combine(storagePath, Path.GetFileName(backupPath));
                File.Move(backupPath, originalPath);
            }
        }
        public static async Task RestoreFile(string currentFilePath,string newFilePath)
        {
            if (File.Exists(currentFilePath))
            {
                File.Move(currentFilePath, newFilePath);
            }
        }
    }
}
