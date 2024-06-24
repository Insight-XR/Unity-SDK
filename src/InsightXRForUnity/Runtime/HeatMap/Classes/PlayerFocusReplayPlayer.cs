using InsightXR.Channels;
using UnityEngine;

public class PlayerFocusReplayPlayer : MonoBehaviour {
    [SerializeField] PlayerFocusRayCaster playerFocusRayCaster;
    [SerializeField] ComponentWeb3DataRecievingChannel dataRecievingChannel;

    private void OnEnable() {
        dataRecievingChannel.DistributionRequestEvent += HandleEvent;
    }

    private void OnDisable() {
        dataRecievingChannel.DistributionRequestEvent -= HandleEvent;
    }

   void HandleEvent(string name, ObjectData objectData){
        if(name == "Main Camera"){
            playerFocusRayCaster.DrawRayForFrame(objectData);
        }
   }
}