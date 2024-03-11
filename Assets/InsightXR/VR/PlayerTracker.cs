// using System.IO;
// using UnityEngine;
// using InsightXR.VR;
// using Newtonsoft.Json;
// using InsightXR.Network;
// using System.Collections.Generic;
// using UnityEngine.XR.Interaction.Toolkit;
// public class PlayerTracker : MonoBehaviour
// {
//     public ActionBasedController LeftHand;
//     public ActionBasedController RightHand;
//     public Camera XRhead;
//     public TriggerInputDetector InputDetector;
//
//     private Transform _Camtans;
//     private RaycastHit hit;
//     private Ray r;
//     public bool _recording;
//     private List<VRPlayerRecord> Movementrecord;
//     public DataHandleLayer Objectcollect;
//     private string path;
//     public int trackerupdate;
//
//     private void OnEnable()
//     {
//         //Set active depending if everything is assigned
//         this.enabled = CheckRefs();
//         // if (XRhead.GetComponent<InsightXR.Core.Component>() != null)
//         // {
//         //     XRhead.AddComponent<InsightXR.Core.Component>();
//         // }
//     }
//
//     void Start()
//     {
//
//         path = Application.dataPath;
//         //resetting all default values
//         _Camtans = XRhead.transform;
//         Movementrecord = new List<VRPlayerRecord>();
//         
//         // status.text = "Simulation";
//         // status.color = Color.yellow;
//
//         //Creating directory if id does not exist
//         if (!Directory.Exists(path + "/Saves"))
//         {
//             Directory.CreateDirectory(path + "/Saves");
//         }
//         
//     }
//
//     // Update is called once per frame
//
//     private void FixedUpdate()
//     {
//         
//         if (InputDetector.GetLeftPrimaryDown())
//         {
//             _recording = !_recording;
//             Debug.Log("Movement Record has : " + Movementrecord.Count);
//
//             if (_recording)
//             {
//                 Debug.Log("Recording Started");
//                 // status.text = "Recording";
//                 // status.color = Color.red;
//                 Movementrecord.Clear();
//                 Objectcollect.enabled = true;
//                 Objectcollect.StartRecording();
//                 
//             }
//             else
//             {
//                 Debug.Log("Player "+trackerupdate);
//                 Debug.Log("Recording Done");
//                 // status.text = "Simulation";
//                 // status.color = Color.yellow;
//                 Objectcollect.StopRecording();
//                 
//                 Debug.Log("Camera Records: " + Movementrecord.Count);
//                 //Debug.Log("Objects Record: "+ Objectcollect.UserInstanceData["Cardboard Box_1"].Count);
//                 MotionPackage DATA = new MotionPackage();
//                 DATA.objectdata = Objectcollect.GetObjectData();
//                 Objectcollect.enabled = false;
//
//                 DATA.Playerdata = JsonConvert.SerializeObject(Movementrecord);
//
//                 //Saving all the data to a file
//                 File.WriteAllText(path + "/Saves/Save.json",JsonConvert.SerializeObject(DATA));
//                 Debug.Log("Saved Movement Data");
//                 
//                 Debug.Log(DATA.objectdata);
//
//                 //Objectcollect.gameObject.GetComponent<NetworkUploader>().UploadFileToServerAsync(JsonConvert.SerializeObject(DATA));
//             }
//         }
//
//
//         if (_recording)
//         {
//             Movementrecord.Add(new VRPlayerRecord(_Camtans.position, _Camtans.rotation));
//             trackerupdate++;
//         }
//     }
//
//     bool CheckRefs()
//     {
//         //Check if all the components are assigned
//         string msg = "";
//         
//         if (LeftHand == null)
//         {
//             msg += " [Left Controller] ";
//         }
//
//         if (RightHand == null)
//         {
//             msg += " [Right Controller] ";
//         }
//
//         if (XRhead == null)
//         {
//             msg += " [Head Camera] ";
//         }
//
//         if (msg == "")
//         {
//             Debug.Log("All Reference Connected");
//             return true;
//         }
//         else
//         {
//             //I could use Debug.logerror here, but the project has many errors, you cant see this one
//             Debug.Log(msg+" Not Assigned");
//             Debug.Log("Deactivating Tracking Component");
//             return false;
//         }
//     }
// }