using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;


using UnityEditor;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace InsightXR.Network
{
    public class NetworkUploader : MonoBehaviour
    {
        public IAmazonS3 s3Client;
        private string awsAccessKeyId = "AKIAXQOF2EHXCW6YVEPP";
        private string awsSecretAccessKey = "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";
        private string bucketName = "shyreyanshaws";

        private void Start()
        {
            //UnityInitializer.AttachToGameObject(gameObject);
            AmazonS3Config s3Config = new AmazonS3Config();
            s3Config.RegionEndpoint = Amazon.RegionEndpoint.APSouth1;

            // Building S3 client with Access Key and Secret Access Key
            s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, s3Config);
            // UploadFileToServerAsync();
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

        //This funtion takes up the file from a location and put it on configured s3 bucket.
        public async void UploadFileToServerAsync(Dictionary<string, List<ObjectData>> UserInstanceData)
        {
            string data = JsonConvert.SerializeObject(UserInstanceData);
            Debug.Log("Creating stream");
            Debug.Log(data);
            string uploadFileName = "Replay Data.json"; // Set a meaningful filename
            string uploadThis = data;
            //var stream = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] cata = Encoding.UTF8.GetBytes(uploadThis);
            var uploadStream = new MemoryStream(cata);


            bool uploaded = await UploadFileAsync(s3Client, bucketName, uploadFileName, uploadStream);
            print($"Uploaded: {uploaded}");
            Application.Quit();

           
        }
        public void DownloadFileToServerAsync()
        {

        }

    }
}