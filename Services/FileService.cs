using System.Security.Cryptography;

namespace DocumentFlow.Services
{
    public interface IFileService
    {
        Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, int documentId);
        Task<byte[]?> GetFileAsync(string filePath);
        void DeleteFile(string filePath);
        Task<string> CalculateHashAsync(IFormFile file);
        bool IsAllowedFileType(string fileName);
        string GetContentType(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50 MB
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".rtf", ".odt" };

        public FileService(IWebHostEnvironment environment)
        {
            _uploadPath = Path.Combine(environment.ContentRootPath, "Uploads", "Documents");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<(string FilePath, string FileName)> SaveFileAsync(IFormFile file, int documentId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл не выбран");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"Размер файла превышает {_maxFileSize / 1024 / 1024} МБ");

            if (!IsAllowedFileType(file.FileName))
                throw new ArgumentException("Недопустимый тип файла");

            var documentFolder = Path.Combine(_uploadPath, documentId.ToString());
            if (!Directory.Exists(documentFolder))
            {
                Directory.CreateDirectory(documentFolder);
            }

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(documentFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (filePath, file.FileName);
        }

        public async Task<byte[]?> GetFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath);
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<string> CalculateHashAsync(IFormFile file)
        {
            using var sha256 = SHA256.Create();
            using var stream = file.OpenReadStream();
            var hash = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hash);
        }

        public bool IsAllowedFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && _allowedExtensions.Contains(extension);
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                ".odt" => "application/vnd.oasis.opendocument.text",
                _ => "application/octet-stream"
            };
        }
    }
}
