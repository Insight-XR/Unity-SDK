// using System;
// using Firebase;
// using System.IO;
// using UnityEngine;
// using Firebase.Storage;
// using Firebase.Database;
// using System.Threading.Tasks;
// using Firebase.Extensions;
//
// public class FirebaseFileUploader : MonoBehaviour
// {
//     public static FirebaseFileUploader Instance;
//     private FirebaseStorage storage         = default;
//     private void Awake(){
//         if  (Instance != null && Instance != this) Destroy(this);
//         else Instance = this;
//     }
//
//     private void Start()
//     {
//         FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
//         {
//             if (task.Result.Equals(DependencyStatus.Available)){
//                 storage     = FirebaseStorage.DefaultInstance;
//                 Debug.Log("Successfully initialize the database.");
//             }
//             else
//             {
//                 Debug.LogError("Failed to initialize Firebase.");
//             }
//         });
//     }
//     public async Task UploadByteArray(byte[] byteArray, string storagePath)
//     // uploading file to firebase storage.
//     {
//         if (storage == null)
//         {
//             Debug.LogError("Firebase Storage is not initialized.");
//             return;
//         }
//
//         // Convert byte array to stream
//         using MemoryStream stream = new(byteArray);
//         // Create a storage reference
//         StorageReference storageRef = storage.GetReference(storagePath);
//
//         // Upload the file
//         await storageRef.PutStreamAsync(stream);
//         Debug.Log("File uploaded to Firebase Storage.");
//     }
//
//     // download a file from firebase storage
//     public async Task DownloadFile(string storagePath, string localFilePath)
//     {
//         // reference to the file in firebase storage
//         StorageReference storageRef = storage.GetReference(storagePath);
//
//         // download the file as a byte array
//         const long maxDownloadSize = 10 * 1024 * 1024; // Maximum download size in bytes (e.g., 10 MB)
//         byte[] fileBytes = await storageRef.GetBytesAsync(maxDownloadSize);
//
//         // save the byte array to a local file
//         File.WriteAllBytes(localFilePath, fileBytes);
//
//         Debug.Log($"File downloaded successfully to: {localFilePath}");
//     }
//
//     public async Task<byte[]> DownloadByteArray(string storagePath)
//     {
//         if (storage == null)
//         {
//             Debug.LogError("Firebase Storage is not initialized.");
//             return null;
//         }
//
//         // Create a storage reference
//         Firebase.Storage.StorageReference storageRef = storage.GetReference(storagePath);
//
//         // Download the file as a stream
//         var task = storageRef.GetStreamAsync();
//
//         await task;
//
//         if (task.IsFaulted || task.IsCanceled)
//         {
//             Debug.LogError("Failed to download file.");
//             return null;
//         }
//         else
//         {
//             // Read the stream into a byte array
//             using (MemoryStream ms = new MemoryStream())
//             {
//                 await task.Result.CopyToAsync(ms);
//                 return ms.ToArray();
//             }
//         }
//     }
// }
