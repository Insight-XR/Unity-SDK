// using System.IO;
// using UnityEngine;
// using System.Collections.Generic;
// using System.Runtime.Serialization.Formatters.Binary;
//
// [DefaultExecutionOrder (-100)]
// public class DataManager : MonoBehaviour
// {
//     [Header("Broadcasting to")]
//     [SerializeField] private DataDistributionChannel        DataDistributionChannel     = default;
//     [Header("Listening to")]
//     [SerializeField] private DataCollectionChannel          DataCollectionChannel       = default;
//     private Dictionary<string, List<ActionReplayRecord>>    DataStorage                 = default;
//     [SerializeField] private int dataCountOnObject;
//     private string filePath;
//     
//     private void Awake(){
//         DataStorage = new();
//         filePath    = Application.persistentDataPath + "/data.dat";
//         DataCollectionChannel.CollectionRequestEvent += StoreData;
//     }
//     private void OnDisable() => DataCollectionChannel.CollectionRequestEvent -= StoreData;
//     private void StoreData(string Uid, List<ActionReplayRecord> ActionReplayRecords)
//     {
//         if (DataStorage.Count == dataCountOnObject)
//         {
//             DataStorage.Clear();
//         }
//         // if (DataStorage.ContainsKey(Uid)){
//         //     DataStorage[Uid] = ActionReplayRecords;
//         // }else{
//         // }
//         DataStorage.Add(Uid, ActionReplayRecords);
//
//         if(DataStorage.Count == dataCountOnObject){
//             SaveByteArrayToFile(ConvertToByteArray(), filePath);
//         }
//     }
//     /*
//     /TEMP DEBUG SECTION/
//     */
//     private void Update(){
//         //THIS WILL PULL THE FILE FROM THE SERVER.
//         if (Input.GetKeyDown(KeyCode.P))
//         {
//             Debug.Log("Downloading the file from server");
//             DownloadFile();
//         }
//     }
//     private async void DownloadFile(){
//         try
//         {
//             // Call the DownloadFile function and wait for its completion
//             byte[] data = await FirebaseFileUploader.Instance.DownloadByteArray("test");
//             Debug.Log("File has downloaded successfully");
//             ConvertFromByteArray(data);
//             DataDistributionChannel.RaiseEvent(DataStorage);
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"An error occurred during download: {ex.Message}");
//         }
//     }
//
//     ////////////////////////////////////////////////////////////////////////////////////////////////////////////
//     //////////////////////////////////////////SAVING SECTION///////////////////////////////////////////////////
//     //////////////////////////////////////////////////////////////////////////////////////////////////////////
//     private byte[] ConvertToByteArray()
//     {
//         BinaryFormatter bf      = new();
//         using MemoryStream ms   = new();
//         bf.Serialize(ms, DataStorage);
//         return ms.ToArray();
//     }
//     private void SaveByteArrayToFile(byte[] byteArray, string filePath) 
//     { 
//         File.WriteAllBytes(filePath, byteArray);
//         // FirebaseFileUploader.Instance.UploadFile(filePath, "test");
//         _ = FirebaseFileUploader.Instance.UploadByteArray(byteArray, "test");
//         Debug.Log("Uploading the file to server");
//     }
//     ////////////////////////////////////////////////////////////////////////////////////////////////////////////
//     //////////////////////////////////////////LOADING SECTION//////////////////////////////////////////////////
//     //////////////////////////////////////////////////////////////////////////////////////////////////////////
//     // private byte[] LoadByteArrayFromFile()
//     // {
//     //     if (File.Exists(filePath))
//     //         return File.ReadAllBytes(filePath);
//     //     else
//     //         Debug.LogError("File not found: " + filePath);
//     //         return null;
//     // }
//     private void ConvertFromByteArray(byte[] data)
//     {
//         BinaryFormatter bf      = new();
//         using MemoryStream ms   = new(data);
//         DataStorage             = (Dictionary<string, List<ActionReplayRecord>>)bf.Deserialize(ms);
//     }
// }
