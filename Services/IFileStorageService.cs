namespace ETSU_Marketplace.Services;
public interface IFileStorageService
{
    public Task<string> ProcessImageUpload(IFormFile file);
    public void DeleteImage(string relativePath);
}