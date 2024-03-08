using System.Collections;
using Amazon;
using Amazon.S3;
using UnityEngine;
using System.IO;
using System.Text;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.CognitoIdentity;
using Unity.VisualScripting;
using UnityEditor;

namespace InsightXR.Network
{
    public class NetworkUploader : MonoBehaviour
{
    public static AmazonS3Client s3Client;
    private AWSCredentials awsCredentials;
    private readonly string IdentityPoolId = "ap-south-1:d3e95916-15f4-439b-a392-32e1b5d94480";

    private void Start(){
        UnityInitializer.AttachToGameObject(gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        
        AWSConfigs.LoggingConfig.LogTo          = LoggingOptions.UnityLogger;
        AWSConfigs.LoggingConfig.LogResponses   = ResponseLoggingOption.Always;
        AWSConfigs.LoggingConfig.LogMetrics     = true;
        AWSConfigs.CorrectForClockSkew          = true;
        
        awsCredentials  = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint.APSouth1);
        s3Client        = new AmazonS3Client(awsCredentials, RegionEndpoint.APSouth1);
        // UploadFileToServerAsync();
    }

    //This funtion takes up the file from a location and put it on configured s3 bucket.
     public void UploadFileToServerAsync(string data)
     {
         string fileName     = "Replay Data"; // Set a meaningful filename
         string uploadThis   = data;
         //var stream = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
    
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
#if UNITY_EDITOR
                 UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
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
    public void DownloadFileToServerAsync(){

    }
    
}
}
