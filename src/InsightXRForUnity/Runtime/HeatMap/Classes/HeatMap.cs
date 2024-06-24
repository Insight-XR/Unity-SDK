using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeatMap {
    private Dictionary<Vector3,float> _rawHeatMap = new();
    public Dictionary<Vector3,float> RawMap => _rawHeatMap;

    private Dictionary<Vector3, float> _processedMap = new();

    private float _pointRadius = 0.05f;
    private float _fallOffExponent = 1f;
    private bool _removeCoolPoints;

    private Vector3 _lastPoint;

    public float GetHeatAtPoint(Vector3 position){
        if(_rawHeatMap.ContainsKey(position) == false) {
            Debug.Log("Point not found in heat map hence 0");
            return 0f;
        }
        return _rawHeatMap[position];
    }

    public void  ApplyCooldown(float coolBy){
        var coolPoints = new List<Vector3>();
        var hotPoints = new Dictionary<Vector3,float>();
        foreach(var heatSource in _rawHeatMap){
            var cooled = heatSource.Value - coolBy;
            if(cooled <= 0) coolPoints.Add(heatSource.Key);
            else hotPoints[heatSource.Key] = cooled;
        }

        foreach(var hotPoint in hotPoints){
            _rawHeatMap[hotPoint.Key] = hotPoint.Value;
        }

        if(!_removeCoolPoints) return;
        foreach(var coolPoint in coolPoints){
            _rawHeatMap.Remove(coolPoint);
        }
    }

    public void AddHeatToPoint(Vector3 point, float value){
        var prevPoint  = new Vector3(_lastPoint.x,_lastPoint.y,_lastPoint.z);
        _lastPoint = point;
        if(_rawHeatMap.ContainsKey(point)) {
            _rawHeatMap[point] += value;
            return;
        }        
        _rawHeatMap[point] = value;
    }

    public Dictionary<Vector3, float> GenerateProcessedHeatMap(){
        _processedMap = new Dictionary<Vector3,float>(_rawHeatMap);
        if(_processedMap.Count == 0) return _processedMap;
        foreach(var heat in _rawHeatMap){
            var insertedPoints = GetPointsBetween(heat.Key, GetClosestHeatDataInMap(heat.Key).position);
            foreach(var insertedPoint in insertedPoints){
                Debug.Log($"Adding point in processed Map: {insertedPoint}");
                AddPointInProcessedMap(insertedPoint, 0.1f);
            }
        }
        return _processedMap;
    }

    private List<Vector3> GetPointsBetween(Vector3 point1, Vector3 point2){
        var points = new List<Vector3>();
        var distance = Vector3.Distance(point1, point2);
        if (distance <= 2 * _pointRadius) return points;
        var numberOfPoints = Mathf.CeilToInt((distance - 2 * _pointRadius) / (_pointRadius * 2));
        for (int i = 1; i <= numberOfPoints; i++) {
            var t = i / (float)(numberOfPoints + 1);
            var p = Vector3.Lerp(point1, point2, t);
            points.Add(p);
         }
        return points;
    }
    
    public HeatPoint GetClosestHeatDataInMap(Vector3 point) {
        if(_rawHeatMap.Count == 0) throw new System.Exception("Heat map is empty hence cannot get the closest Heat value");
        if(_rawHeatMap.Count == 1) return new HeatPoint(_rawHeatMap.FirstOrDefault().Key, _rawHeatMap.FirstOrDefault().Value);
        float minDistance = float.PositiveInfinity;
        Vector3 closestPoint = Vector3.zero;
        foreach(var kvp in _rawHeatMap){
            if(kvp.Key == point) continue;
            if(Vector3.Distance(kvp.Key, point) < minDistance){
                minDistance = Vector3.Distance(kvp.Key, point);
                closestPoint = kvp.Key;
            }
        }
        
        return new HeatPoint(closestPoint, _rawHeatMap[closestPoint]);
    }

    private void AddPointInProcessedMap(Vector3 point, float heat){
        var heatValue = heat;
        foreach(var heatSource in _rawHeatMap){
            var distance = Vector3.Distance(point,heatSource.Key);
            var heatContribution = heatSource.Value / Mathf.Pow(distance, _fallOffExponent);
            heat += heatContribution;
        }
        _processedMap[point] = heatValue;
    }
}

public class HeatPoint{
    public Vector3 position;
    public float heat;

    public HeatPoint(Vector3 point, float heat){
        this.position = point;
        this.heat = heat;
    }
}

