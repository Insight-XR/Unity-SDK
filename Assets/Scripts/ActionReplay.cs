using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ActionReplay : MonoBehaviour
{
    [Header("Broadcasting to")]
    [SerializeField] private DataCollectionChannel      DataCollectionChannel   = default;
    [Header("Listening to")]
    [SerializeField] private InternalEventChannel       ShotEventChannel        = default;
    [SerializeField] private DataDistributionChannel    DataDistributionChannel = default;
    private List<ActionReplayRecord>                    actionReplayRecords     = new();
    private new Rigidbody                               rigidbody               = default;
    private float                                       currentReplayIndex;
    private float                                       indexChangeRate;
    private bool                                        isInReplayMode;
    private bool                                        canShot;

    public TMP_Text status;
    public XRController left;
    private void Start(){
        rigidbody = GetComponent<Rigidbody>();
        ShotEventChannel.NetworkCallbackRequested           += ProceedDataCollection;
        DataDistributionChannel.DistributionRequestEvent    += CollectDataFromSaveFile;

        status = GameObject.Find("Status").GetComponent<TMP_Text>();
    }
    private void OnDisable() { 
        ShotEventChannel.NetworkCallbackRequested           -= ProceedDataCollection;
        DataDistributionChannel.DistributionRequestEvent    -= CollectDataFromSaveFile;
    }
    private void ProceedDataCollection(bool shot) => canShot = shot;
    
    private void CollectDataFromSaveFile(Dictionary<string, List<ActionReplayRecord>> data){
        isInReplayMode          = true;
        rigidbody.isKinematic   = isInReplayMode;

        foreach (var entry in data){
            if(entry.Key.Equals(gameObject.GetInstanceID().ToString())){
                actionReplayRecords = entry.Value;
                Debug.Log("updated the data for the gameobject, Ready in reply mode");
            }
        }
    }
    private void Update(){
        if(Input.GetKeyDown(KeyCode.R)){
            if(isInReplayMode){
                Debug.Log("Already In Replay Mode. Press S to Shoot Again");
                return;
            }
            isInReplayMode = true;
            SetTransform(isInReplayMode ? 0 : actionReplayRecords.Count - 1);
            rigidbody.isKinematic = isInReplayMode;
            DataCollectionChannel.RaiseEvent(gameObject.GetInstanceID().ToString(), actionReplayRecords);
            
            
        }
        if(Input.GetKeyDown(KeyCode.S)){
            SetTransform(0);
            isInReplayMode          = false;
            rigidbody.isKinematic   = isInReplayMode;
            actionReplayRecords     = new();
        }
        indexChangeRate = 0;
        if(Input.GetKey(KeyCode.RightArrow)){
            indexChangeRate = 1;
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            indexChangeRate = -1;
        }
        if(Input.GetKey(KeyCode.LeftShift)){
            indexChangeRate *= 0.5f;
        }
    }
    private void FixedUpdate(){ 
        if(!isInReplayMode && canShot){
            actionReplayRecords.Add(new ActionReplayRecord{position = new SerializableVector3(transform.position), rotation = new SerializableQuaternion(transform.rotation) });
        }
        else{ 
            float nextIndex = currentReplayIndex + indexChangeRate;
            if (nextIndex < actionReplayRecords.Count && nextIndex >= 0) SetTransform(nextIndex);
        }
    }
    private void SetTransform(float index){
        currentReplayIndex = index;
        ActionReplayRecord _actionReplayRecord = actionReplayRecords[(int)index];
        transform.SetPositionAndRotation(_actionReplayRecord.position.ToVector3(), _actionReplayRecord.rotation.ToQuaternion());
    }
}