using Amazon.S3;
using Amazon.S3.Model;

namespace ProductService.Application.Usecases
{
    public class ManageProductImagesUC
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<ManageProductImagesUC> _logger;
        private readonly string _bucketName; // Tên bucket cần được truyền vào hoặc cấu hình


        public ManageProductImagesUC(IAmazonS3 s3Client, ILogger<ManageProductImagesUC> logger, string bucketName)
        {
            _s3Client = s3Client;
            _logger = logger;
            _bucketName = bucketName;

            if (string.IsNullOrEmpty(_bucketName))
            {
                _logger.LogError("Bucket name is not provided for ManageProductImagesUC.");
                throw new ArgumentNullException(nameof(bucketName), "Bucket name must be provided for image management.");
            }
        }

        public async Task<string> UploadImageAsync(
            Stream imageStream,
            string FileName,
            string contentType,
            string subFolder = null,
            bool isPublicRead = true)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                _logger.LogWarning("Attempted to upload an empty image stream.");
                throw new ArgumentException("Image stream cannot be null or empty.", nameof(imageStream));
            }

            if (string.IsNullOrEmpty(FileName))
            {
                _logger.LogWarning("Original file name is empty for image upload.");
                throw new ArgumentException("Original file name cannot be empty.", nameof(FileName));
            }

            if (string.IsNullOrEmpty(contentType))
            {
                _logger.LogWarning("Content type is empty for image upload.");
                throw new ArgumentException("Content type cannot be empty.", nameof(contentType));
            }

            try
            {
            

                // Build the S3 Key (path within the bucket)
                string s3Key;
                if (!string.IsNullOrEmpty(subFolder))
                {
                    s3Key = $"{subFolder.TrimEnd('/')}/{FileName}";
                }
                else
                {
                    s3Key = FileName;
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    ContentType = contentType,
                    InputStream = imageStream,
                };
                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Successfully uploaded {s3Key} to S3 bucket {_bucketName}.");
                    // Construct and return the public URL
                    // Note: Region should ideally come from _s3Client.Config.RegionEndpoint.SystemName
                    // but for quick setup, using a configured region is fine.
                    // Replace "ap-southeast-2" with your actual region from configuration.
                    return $"https://{_bucketName}.s3.ap-southeast-2.amazonaws.com/{s3Key}";
                }
                else
                {
                    _logger.LogError($"Failed to upload {FileName} to S3. HttpStatusCode: {response.HttpStatusCode}");
                    throw new Exception($"Failed to upload image. S3 response status: {response.HttpStatusCode}");
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, $"Amazon S3 error during image upload for {FileName}: {s3Ex.Message}");
                throw new Exception($"S3 Error during image upload: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during image upload for {FileName}: {ex.Message}");
                throw new Exception($"Unexpected error during image upload: {ex.Message}", ex);
            }
        }

 
        public async Task<bool> DeleteImageAsync(string s3Key)
        {
            if (string.IsNullOrEmpty(s3Key))
            {
                _logger.LogWarning("Attempted to delete image with empty S3 Key.");
                throw new ArgumentException("S3 Key cannot be null or empty.", nameof(s3Key));
            }

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key
                };

                DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(deleteRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent ||
                    response.HttpStatusCode == System.Net.HttpStatusCode.OK) // S3 can return 200 or 204
                {
                    _logger.LogInformation($"Successfully deleted {s3Key} from S3 bucket {_bucketName}.");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to delete {s3Key} from S3. HttpStatusCode: {response.HttpStatusCode}");
                    throw new Exception($"Failed to delete image. S3 response status: {response.HttpStatusCode}");
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, $"Amazon S3 error during image deletion for {s3Key}: {s3Ex.Message}");
                throw new Exception($"S3 Error during image deletion: {s3Ex.Message}", s3Ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during image deletion for {s3Key}: {ex.Message}");
                throw new Exception($"Unexpected error during image deletion: {ex.Message}", ex);
            }
        }

        // Bạn có thể thêm các phương thức khác ở đây nếu cần (ví dụ: GetImageMetadataAsync, ListImagesAsync)
    }
}
