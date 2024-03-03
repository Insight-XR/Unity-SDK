using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using UnityEngine;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using System.IO;
using System.Text;
using UnityEditorInternal;

public class NetworkUploader : MonoBehaviour
{
    public static AmazonS3Client s3Client;
    private string IdentityPoolId = "ap-south-1:d3e95916-15f4-439b-a392-32e1b5d94480";
    private AWSCredentials awsCredentials;

    private void Start(){
        UnityInitializer.AttachToGameObject(gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
        AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
        AWSConfigs.LoggingConfig.LogMetrics = true;
        AWSConfigs.CorrectForClockSkew = true;

        awsCredentials = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint.APSouth1);
        s3Client        = new AmazonS3Client(awsCredentials, RegionEndpoint.APSouth1);
        // byte[] sampleData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // Sample byte array representing ASCII characters "Hello"
        // UploadFileToServerAsync(sampleData);
        // GetBucketList();
        UploadFileToServerAsync();
    }
    public void GetBucketList()
    {
        var ResultText = "Fetching all the Buckets";
        s3Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
        {
            ResultText += "\n";
            if (responseObject.Exception == null)
            {
                ResultText += "Got Response \nPrinting now \n";
                responseObject.Response.Buckets.ForEach((s3b) =>
                {
                    ResultText += string.Format("bucket = {0}, created date = {1} \n", s3b.BucketName, s3b.CreationDate);
                });
                Debug.Log(ResultText);
            }
            else
            {
                ResultText += "Got Exception \n";
                Debug.Log(ResultText);
            }
        });
    }

    public void UploadFileToServerAsync()
    {
        string fileName     = "GetFileHelper"; // Set a meaningful filename
        string uploadThis   = "In the heart of an ancient forest, where the trees whispered secrets of old and the air carried tales of forgotten realms, there existed a peculiar clearing. This was not an ordinary clearing, but one that shimmered with an ethereal glow when the moon was full and high in the sky. It was said that this place held the power to bridge worlds";
        byte[] cata         = Encoding.UTF8.GetBytes(uploadThis);
        var uploadStream    = new MemoryStream(cata);
        var request         = new PutObjectRequest{
            BucketName      = AmazonS3.BUCKET_NAME,
            Key             = fileName,
            InputStream     = uploadStream,
            CannedACL       = S3CannedACL.Private
        };

        Debug.Log("Creating request object");

        s3Client.PutObjectAsync(request, (responseObj) =>
        {
            if (responseObj.Exception == null)
            {
                Debug.Log($"Object {responseObj.Request.Key} posted to bucket ");
            }
            else
            {
                Debug.LogError($"Exception while posting the result object: {responseObj.Exception.Message}");
                Debug.LogError($"Received error: {responseObj.Response.HttpStatusCode}");
            }

            // Ensure the memory stream is disposed after use
            uploadStream.Dispose();
        });
    }
}