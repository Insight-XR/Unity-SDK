using System;
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
using Unity.VisualScripting;
using Random = UnityEngine.Random;


namespace InsightXR.Network
{
    public class NetworkUploader : MonoBehaviour
    {
        
        public IAmazonS3 s3Client;
        public string awsAccessKeyId;
        public string awsSecretAccessKey;
        public string bucketName;
        private DataHandleLayer Handler;

        private void Start()
        {
            //UnityInitializer.AttachToGameObject(gameObject);
            AmazonS3Config s3Config = new AmazonS3Config();
            s3Config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;

            // Building S3 client with Access Key and Secret Access Key
            s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, s3Config);
            Handler = FindObjectOfType<DataHandleLayer>();
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

        //This funtion takes up the file from a location and put it on configured s3 bucket.
        public async void UploadFileToServerAsync(SaveData savedata , bool closeapp)
        {
            string data = JsonConvert.SerializeObject(savedata);
            string uploadFileName = savedata.sessionID+".json";
            string uploadThis = data;
            
            byte[] cata = Encoding.UTF8.GetBytes(uploadThis);
            var uploadStream = new MemoryStream(cata);
            
            bool uploaded = await UploadFileAsync(s3Client, bucketName, uploadFileName, uploadStream, savedata.CustomerID);
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
}