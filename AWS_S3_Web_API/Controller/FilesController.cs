using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using AWS_S3_Web_API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AWS_S3_Web_API.Controller;

public class FilesController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IAmazonS3 _amazonS3;

    public FilesController(IAmazonS3 amazonS3)
    {
        _amazonS3 = amazonS3;
    }


    [HttpPost("UploadFile")]
    public async Task<IActionResult> UploadFilesAsync(IFormFile file, string bucketName, string? prefix)
    {
        bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
        if (!bucketExists)
            return NotFound($"Bucket {bucketName} does nor exist");
        PutObjectRequest request = new()
        {
            BucketName = bucketName,
            Key = String.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
            InputStream = file.OpenReadStream()
        };
        request.Metadata.Add("Content-type",file.ContentType);
        await _amazonS3.PutObjectAsync(request);
        return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
    }

    [HttpGet("GetAllFiles")]
    public async Task<IActionResult> GetAllFileAsync(string bucketName, string? prefix)
    {
        bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
        if (!bucketExists)
            return NotFound($"Bucket {bucketName} does nor exist");

        ListObjectsV2Request request = new()
        {
            BucketName = bucketName,
            Prefix = prefix
        };
        ListObjectsV2Response response = await _amazonS3.ListObjectsV2Async(request);
        List<S3ObjectDto> objectDtos = response.S3Objects.Select(s3Object =>
        {
            GetPreSignedUrlRequest urlRequest = new()
            {
                BucketName = bucketName,
                Key = s3Object.Key,
                Expires = DateTime.UtcNow.AddMinutes(2)
            };
            return new S3ObjectDto
            {
                Name = s3Object.Key,
                Url = _amazonS3.GetPreSignedURL(urlRequest)
            };
        }).ToList();
        return Ok(objectDtos);
    }


    [HttpDelete("DeleteFiles")]
    public async Task<IActionResult> DeleteFileAsync(string bucketName, string fileName)
    {
        bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
        if (!bucketExists)
            return NotFound($"Bucket {bucketName} does nor exist");

        await _amazonS3.DeleteObjectAsync(bucketName, fileName);
        return NoContent();
    }

    [HttpGet("download")]
    public async Task<IActionResult> GetFileByNameAsync(string bucketName, string fileName)
    {
        bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
        if (!bucketExists)
            return NotFound($"Bucket {bucketName} does nor exist");

        GetObjectResponse response = await _amazonS3.GetObjectAsync(bucketName, fileName);
        return File(response.ResponseStream, response.Headers.ContentType);
    }
}