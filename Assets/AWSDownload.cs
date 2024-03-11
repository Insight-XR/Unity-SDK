using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using UnityEngine;
using System.Collections;
using System.IO;

public class AWSDownload : MonoBehaviour
{
    public IAmazonS3 s3Client;
    private string bucketName = "shyreyanshaws"; // Replace with your bucket name
    private string keyName = "image_uploaded_with_iam_user.jpg"; // Replace with the key of the object you want to download
    private string localFilePathDownload = "C:\\Users\\reyan\\Experiment\\download.jpg"; // Replace with the path where you want to save the file
    private string localFilePathUpload = "C:\\Users\\aksha\\OneDrive\\Pictures\\Screenshots\\Screenshot 2024-02-22 222240.png"; // Replace with the path where you want to save the file
    private string uploadFileName = "sudhanshucall.png"; // Replace with the file name you want to use
    private string awsAccessKeyId = "AKIAXQOF2EHXCW6YVEPP";
    private string awsSecretAccessKey = "aedSheo5pB98v5KvaXhguSv1zLZ/p3Hzlt0bbSK3";
    public Texture2D image;
    void Start()
    {
        AmazonS3Config s3Config = new AmazonS3Config();
        s3Config.RegionEndpoint = Amazon.RegionEndpoint.APSouth1;

        // Building S3 client with Access Key and Secret Access Key
        s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, s3Config);
        if (image != null)
        {
            Pick();

        }
        else
        {
            Debug.Log("Stuff gone wrong");
        }
    }

    public async void Pick()
    {
        // bool uploaded = await UploadFileAsync(s3Client, bucketName, uploadFileName, localFilePathUpload);
        Debug.Log(image.name);
        byte[] image_bytes = ImageConversion.EncodeToPNG(image);
        MemoryStream image_stream = new MemoryStream(image_bytes);
        bool uploaded = await UploadFileAsync(s3Client, bucketName, uploadFileName, image_stream);
        print($"Uploaded: {uploaded}");
    }

    private static async Task<bool> UploadFileAsync(IAmazonS3 client, string bucketName, string objectName, string filePath)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
            FilePath = filePath,
        };

        var response = await client.PutObjectAsync(request);

        return (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
    }

    private static async Task<bool> UploadFileAsync(IAmazonS3 client, string bucketName, string objectName, Stream fileStream)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
            InputStream = fileStream,
        };

        var response = await client.PutObjectAsync(request);

        return (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
    }
}