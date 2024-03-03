using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using XRController = UnityEngine.InputSystem.XR.XRController;


public class PlayerTracker : MonoBehaviour
{
    public ActionBasedController LeftHand;
    public ActionBasedController RightHand;
    public Camera XRhead;
    public TriggerInputDetector InputDetector;
    public TMP_Text status;

    
    private Transform _Camtans;
    private RaycastHit hit;
    private Ray r;

    public bool _recording;

    private List<VRPlayerRecord> Movementrecord;


    // Start is called before the first frame update

    private void OnEnable()
    {
        //Set active depending if everything is assigned
        this.enabled = CheckRefs();
    }

    void Start()
    {
        //resetting all default values
        _Camtans = XRhead.transform;
        Movementrecord = new List<VRPlayerRecord>();
        
        status.text = "Simulation";
        status.color = Color.yellow;

        //Creating directory if id does not exist
        if (!Directory.Exists(Application.dataPath + "/Saves"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Saves");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (InputDetector.GetLeftPrimaryDown())
        {
            _recording = !_recording;
            Debug.Log("Movement Record has : " + Movementrecord.Count);

            if (_recording)
            {
                Debug.Log("Recording Started");
                status.text = "Recording";
                status.color = Color.red;
                Movementrecord.Clear();
            }
            else
            {
                Debug.Log("Recording Done");
                status.text = "Simulation";
                status.color = Color.yellow;
                
                //Saving all the data to a file
                File.WriteAllText(Application.dataPath + "/Saves/Save.json",JsonConvert.SerializeObject(Movementrecord));
                Debug.Log("Saved Movement Data");
            }
        }


        if (_recording)
        {
            Movementrecord.Add(new VRPlayerRecord(_Camtans.position, _Camtans.rotation));
        }
    }

    bool CheckRefs()
    {
        //Check if all the components are assigned
        string msg = "";
        
        if (LeftHand == null)
        {
            msg += " [Left Controller] ";
        }

        if (RightHand == null)
        {
            msg += " [Right Controller] ";
        }

        if (XRhead == null)
        {
            msg += " [Head Camera] ";
        }

        if (msg == "")
        {
            Debug.Log("All Reference Connected");
            return true;
        }
        else
        {
            //I could use Debug.logerror here, but the project has many errors, you cant see this one
            Debug.Log(msg+" Not Assigned");
            Debug.Log("Deactivating Tracking Component");
            return false;
        }
    }
    
    
}
