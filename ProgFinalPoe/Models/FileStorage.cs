namespace ProgFinalPoe.Models
{
    public class FileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        // file types that are allowed to be uploaded
        private readonly string[] _allowed = new[] { ".pdf", ".docx", ".xlsx", ".png" };
        // 100 MB coverted into bytes
        private const long MaxBytes = 100 * 1024 * 1024;

        public FileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            if (file.Length > MaxBytes)
                throw new InvalidOperationException("File size too large");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (Array.IndexOf(_allowed, ext) < 0)
                throw new InvalidOperationException("Invalid file type. Allowed types: .pdf, .docx, .xlsx, .png");

            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var unique = $"{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(uploads, unique);

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return unique;
        }

        public Task DeleteFile(string filename)
        {
            var path = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", filename);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
