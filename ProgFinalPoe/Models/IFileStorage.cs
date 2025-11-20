namespace ProgFinalPoe.Models
{
    public interface IFileStorage
    {
        Task<string> SaveFile(IFormFile file);
        Task DeleteFile(string filename);
    }
}