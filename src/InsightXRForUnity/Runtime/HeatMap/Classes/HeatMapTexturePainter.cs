using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

public class HeatMapTexturePainter : MonoBehaviour{
    [SerializeField] float minHeatThreshold = 0.1f;

    PlayerFocusRayCaster _playerFocusRayCaster;
    HeatMapController _heatMapController;
    Renderer _renderer;
    Texture2D _heatTexture;
    Gradient _heatMapGradient;
    int _brushRadius = 10;
    Texture2D _brushTexture;

    private List<HeatTextureCoordinates> _registeredHeatPoints = new();

    public void Initialize(Gradient heatMapGradient, Material heatMapMaterial, Texture2D brushTexture){
        _brushTexture = brushTexture;

        _heatMapGradient = heatMapGradient;
        _renderer = GetComponent<Renderer>();
        InitializeMaterial(heatMapMaterial);

        _playerFocusRayCaster = FindObjectOfType<PlayerFocusRayCaster>();
        _heatMapController = FindObjectOfType<HeatMapController>();
        _playerFocusRayCaster.onFocusChanged += RegisterHeatPoint;
        _heatMapController.OnHeatMapUpdate += OnHeatMapUpdate;
    }

    void InitializeMaterial(Material heatMapMaterial){
        var renderer = GetComponent<MeshRenderer>();
        renderer.materials = new Material[]{};
        renderer.AddMaterial(heatMapMaterial);

        InitializeHeatTexture(500, 500);
    }

    void OnDisable() {
        _playerFocusRayCaster.onFocusChanged -= RegisterHeatPoint;
        _heatMapController.OnHeatMapUpdate -= OnHeatMapUpdate;
    }

    public void RegisterHeatPoint(RaycastHit hit,Vector3 heatMapCoordinate) {
        if(hit.collider.gameObject != gameObject || _heatTexture == null) {
            return;
        }
        Debug.Log($"Hit registered on {gameObject.name} at {hit.textureCoord}");

        var pixelUV = new Vector2Int{
            x = (int)(hit.textureCoord.x * _heatTexture.width),
            y = (int)(hit.textureCoord.y * _heatTexture.height)
        };
        _registeredHeatPoints.Add(new HeatTextureCoordinates(pixelUV, heatMapCoordinate));
    }

    private void OnHeatMapUpdate(HeatMap heatMap){
        List<HeatTextureCoordinates> coolPoints = new();
        foreach(var heatCoordinate in _registeredHeatPoints){
            var heat = heatMap.GetHeatAtPoint(heatCoordinate.worldCoordinate);
            PaintHeat(heatCoordinate, heat, _brushRadius, _brushTexture);
            if(heat < minHeatThreshold){
                coolPoints.Add(heatCoordinate);
                continue;
            }
        }
        foreach(var coolPoint in coolPoints){
            _registeredHeatPoints.Remove(coolPoint);
        }
    }

private void PaintHeat(HeatTextureCoordinates heatTextureCoordinates, float heatValue, int radius, Texture2D brushTexture) {
    Color heatColor = _heatMapGradient.Evaluate(heatValue);
    int centerX = heatTextureCoordinates.uvCoordinate.x;
    int centerY = heatTextureCoordinates.uvCoordinate.y;

    int brushWidth = brushTexture.width;
    int brushHeight = brushTexture.height;

    int startX = Mathf.Max(centerX - radius, 0);
    int startY = Mathf.Max(centerY - radius, 0);
    int endX = Mathf.Min(centerX + radius, _heatTexture.width - 1);
    int endY = Mathf.Min(centerY + radius, _heatTexture.height - 1);

    float radiusSquared = radius * radius;

    for (int x = startX; x <= endX; x++) {
        for (int y = startY; y <= endY; y++) {
            float distanceSquared = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
            if (distanceSquared <= radiusSquared) {
                float brushX = Mathf.InverseLerp(centerX - radius, centerX + radius, x) * (brushWidth - 1);
                float brushY = Mathf.InverseLerp(centerY - radius, centerY + radius, y) * (brushHeight - 1);

                float brushAlpha = brushTexture.GetPixelBilinear(brushX / brushWidth, brushY / brushHeight).a;

                // Use the alpha value as a clipping mask
                if (brushAlpha > 0f) {
                    Color currentColor = _heatTexture.GetPixel(x, y);
                    Color blendedColor = Color.Lerp(currentColor, heatColor, brushAlpha);
                    _heatTexture.SetPixel(x, y, blendedColor);
                }
            }
        }
    }

    _heatTexture.Apply();
}



private void InitializeHeatTexture(int width, int height){
        _heatTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color initialColor = _heatMapGradient.Evaluate(0f);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                _heatTexture.SetPixel(x, y, initialColor);
            }
        }
        _heatTexture.Apply();
        _renderer.materials.Last().mainTexture = _heatTexture;
    }
}

public class HeatTextureCoordinates {
    public Vector2Int uvCoordinate;
    public Vector3 worldCoordinate;

    public HeatTextureCoordinates(Vector2Int uvCoordinate, Vector3 worldCoordinate) {
        this.uvCoordinate = uvCoordinate;
        this.worldCoordinate = worldCoordinate;
    }
}
