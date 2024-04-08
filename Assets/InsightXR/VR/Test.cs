using System.IO;
using UnityEngine;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;

public class FileCreator : MonoBehaviour
{
    
    public IAmazonS3 s3Client;
    public string awsAccessKeyId = "AKIAXQOF2EHXJZCDKVX5";
    public string awsSecretAccessKey = "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";
    public string BucketName = "sessions-3d-data";

    private void Start()
    {
        //UnityInitializer.AttachToGameObject(gameObject);
        AmazonS3Config s3Config = new AmazonS3Config();
        s3Config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;

        // Building S3 client with Access Key and Secret Access Key
        s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, s3Config);
    
        
        
        
        string persistentDataPath = Application.persistentDataPath;

        // Create the file path by combining the persistent data path and the file name
        string filePath = Path.Combine(persistentDataPath, "hello.txt");

        // Write the content to the file
        string content = "Hello, world!";
        File.WriteAllText(filePath, content);

        // Log the file path for debugging purposes
        Debug.Log("File created at: " + filePath);
        
        UploadFileToServerAsync(content, true);
        
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private static async Task<bool> UploadFileAsync(IAmazonS3 client, string bucketName, string objectName, Stream fileStream, string CustomerID)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = CustomerID+"/"+objectName,
            InputStream = fileStream,
        };

        var response = await client.PutObjectAsync(request);

        return (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
            
    }

    
    public async void UploadFileToServerAsync(string savedata , bool closeapp)
    {
        string data = savedata;
        string uploadFileName = "UploadedFile"+".json";
        string uploadThis = data;
            
        byte[] cata = Encoding.UTF8.GetBytes(uploadThis);
        var uploadStream = new MemoryStream(cata);
            
        bool uploaded = await UploadFileAsync(s3Client, BucketName, uploadFileName, uploadStream, 10017.ToString());
        Debug.Log($"Upload Status: {uploaded}");

        if (closeapp)
        {
            if (Application.platform == RuntimePlatform.LinuxEditor ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
            {
                Debug.Log("Application Quit");
            }
            else
            {
                Application.Quit();
            }
        }


    }
}