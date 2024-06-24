using System;
using UnityEngine;

public class PlayerFocusRayCaster : MonoBehaviour {
    [SerializeField] private string playerHeadObjectName;
    [SerializeField] private LineRenderer focusLineRenderer;
    [SerializeField] private float focusLineDistance;
    [SerializeField] private LayerMask focusLayerMask;
    [SerializeField] private GameObject focusPointGizmo;
    [SerializeField] private float focusShiftThreshold = 0.25f;

    private Vector3 _lastFocusPoint;

    public event Action<RaycastHit, Vector3> onFocusChanged;

    void Awake(){
        Initialize();
        HideRay();
    }

    void Initialize(){
        var playerHeadTransform = GameObject.Find(playerHeadObjectName).transform;
        transform.position = playerHeadTransform.position;
        transform.rotation = playerHeadTransform.rotation;
        transform.localScale = playerHeadTransform.localScale;
    }

    public void DrawRayForFrame(ObjectData objectData){
        var frameData = objectData;
        focusLineRenderer.transform.localPosition = frameData.GetPosition();
        Debug.Log($"frame data from frame Data: {frameData.GetPosition()}");
        var start = Vector3.zero;
        var direction = frameData.GetRotation() * Vector3.forward;
        var end = start + direction * focusLineDistance;
        DrawRay(start,end);

        var worldStart = transform.TransformPoint(frameData.GetPosition());
        var worldEnd = worldStart + transform.rotation * direction * focusLineDistance;
        RaycastHit rayCastHit;
        Vector3 hitPosition;
        if(PerformRaycast(worldStart, worldEnd, out rayCastHit, out hitPosition)){
           if(Vector3.Distance(hitPosition, _lastFocusPoint) >= focusShiftThreshold) {
                ShowGraphicsAtFocusPoint(hitPosition);
                onFocusChanged?.Invoke(rayCastHit,hitPosition);
                _lastFocusPoint = hitPosition;
           }
        }
    }


    void ShowGraphicsAtFocusPoint(Vector3 focusPoint){
        focusPointGizmo.SetActive(false);
        if(focusPoint == Vector3.zero) return;
        focusPointGizmo.SetActive(true);
        focusPointGizmo.transform.position = focusPoint; 
    }

    void HideRay(){
        focusLineRenderer.gameObject.SetActive(false);
    }

    void DrawRay(Vector3 start, Vector3 end){
        focusLineRenderer.gameObject.SetActive(true);
        focusLineRenderer.positionCount = 2;
        focusLineRenderer.SetPosition(0, start);
        focusLineRenderer.SetPosition(1, end);
    }

    public bool PerformRaycast(Vector3 startPosition, Vector3 endPosition, out RaycastHit finalHit, out Vector3 finalHeatPoint){
        Vector3 direction = endPosition - startPosition;
        float distance = direction.magnitude;
        direction.Normalize();
        RaycastHit hit;

        if (Physics.Raycast(startPosition, direction, out hit, distance, focusLayerMask)){
            finalHit = hit;
            finalHeatPoint = hit.point - direction * 0.1f;
            return true;
        }
        else{
            finalHit = hit;
            finalHeatPoint = Vector3.zero;
            return false;
        }
    }
}
