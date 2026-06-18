using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HouseBookingRestApi.Exceptions;

namespace HouseBookingRestApi.Service
{
    public class CloudinaryImageStorageService : IImageStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageStorageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(int houseId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = $"house_{houseId}/{Guid.NewGuid()}", // Unique identifier for the image
                Overwrite = true
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ImageUploadException($"Image upload failed: {uploadResult.Error?.Message}");
            }
            return uploadResult.SecureUrl.ToString();
        }
    }
}
