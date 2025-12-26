using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

// service hỗ trợ các hành động upload và download với s3
public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration configuration)
    {
        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"],
            RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
        );
        _bucketName = configuration["AWS:BucketName"];
    }

    public async Task<string> UploadFileAsync(string filePath, string keyName)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(filePath, _bucketName, keyName);

        return $"https://{_bucketName}.s3.amazonaws.com/{keyName}";
    }

    public async Task DeleteFileAsync(string keyName)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        }
        catch (AmazonS3Exception s3Ex)
        {
            Console.WriteLine($"Lỗi khi xóa file S3: {s3Ex.Message}");
            throw;
        }
    }
}