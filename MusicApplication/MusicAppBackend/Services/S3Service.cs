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
    private readonly string _serviceUrl;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client; // Dùng chung IAmazonS3 được khởi tạo ở Program.cs
        _bucketName = configuration["AWS:BucketName"];
        _serviceUrl = configuration["AWS:ServiceURL"];
    }

    public async Task<string> UploadFileAsync(string filePath, string keyName)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(filePath, _bucketName, keyName);

        // Format URL của MinIO: http://ip:9000/bucket_name/key_name
        return $"{_serviceUrl}/{_bucketName}/{keyName}";
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