namespace ETSU_Marketplace.Services;

public class FileStorageService : IFileStorageService
{

    public async Task<string> ProcessImageUpload(IFormFile file)
    {
        // Define where to save (relative to the project root)
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "listings");

        // Ensure the directory exists
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        // Create a unique filename: "a2b3-c4d5.jpg"
        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save the file physically
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return the relative path for the Database
        // We store "/images/listings/uniqueName.jpg" so the browser can find it later
        return $"/images/listings/{uniqueFileName}";
    }

    public void DeleteImage(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains("placeholder")) return;

        // Convert "/images/listings/file.jpg" to a physical path
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}