using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace AWS_S3_Web_API.Controller;

public class BucketController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IAmazonS3 _amazonS3;

    public BucketController(IAmazonS3 amazonS3)
    {
        _amazonS3 = amazonS3;
    }

    [HttpPost("CreateBucket")]
    public async Task<IActionResult> CreateBucketAsync(string bucketName)
    {
        bool bucketExists = await _amazonS3.DoesS3BucketExistAsync(bucketName);
        if (bucketExists)
            return BadRequest($"Bucket {bucketName} already exist.");

        await _amazonS3.PutBucketAsync(bucketName);
        return Ok($"Bucket  {bucketName} created");
    }

    [HttpGet("GetAllBucket")]
    public async Task<IActionResult> GetAllBucketAsync()
    {
        ListBucketsResponse bucketsResponse = await _amazonS3.ListBucketsAsync();
        return Ok(bucketsResponse);
    }


    [HttpDelete("DeleteBucket")]
    public async Task<IActionResult> DeleteBucketAsync(string bucketName)
    {
        await _amazonS3.DeleteBucketAsync(bucketName);
        return NoContent();
    }
}