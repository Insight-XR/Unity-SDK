using System;
using UnityEngine;

public class HeatMapController : MonoBehaviour{
    [SerializeField] PlayerFocusRayCaster playerFocusRayCaster;    
    public HeatMap heatMap = new();

    public event Action<HeatMap> OnHeatMapUpdate;

    private void OnEnable() {
        playerFocusRayCaster.onFocusChanged += OnFocusChangedToPoint;
    }

    private void OnDisable() {
        playerFocusRayCaster.onFocusChanged -= OnFocusChangedToPoint;
    }

    private void OnFocusChangedToPoint(RaycastHit hit, Vector3 point){
        heatMap.AddHeatToPoint(point, 1.1f);
        heatMap.ApplyCooldown(0.007f);
        OnHeatMapUpdate?.Invoke(heatMap);
    }
}