using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatmapVisualization : MonoBehaviour
{
    // Reference to the UI image representing the heatmap
    public Image heatmapImage;

    // Heatmap texture
    private Texture2D heatmapTexture;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize heatmap texture
        InitializeHeatmapTexture();
    }

    // Function to initialize the heatmap texture
    void InitializeHeatmapTexture()
    {
        // Create a new texture for the heatmap with desired width and height
        heatmapTexture = new Texture2D(Screen.width, Screen.height);

        // Clear the texture (set all pixels to transparent)
        Color[] clearPixels = new Color[heatmapTexture.width * heatmapTexture.height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        heatmapTexture.SetPixels(clearPixels);
        heatmapTexture.Apply();

        // Set the heatmap texture to the UI image
        heatmapImage.sprite = Sprite.Create(heatmapTexture, new Rect(0, 0, heatmapTexture.width, heatmapTexture.height), Vector2.zero);
    }

    // Function to update heatmap data at a specific position
    public void UpdateHeatmap(Vector2 position, Color color, float intensity)
    {
        // Convert position to texture coordinates
        Vector2 textureCoord = new Vector2(position.x / Screen.width, position.y / Screen.height);

        // Calculate pixel coordinates
        int x = Mathf.FloorToInt(textureCoord.x * heatmapTexture.width);
        int y = Mathf.FloorToInt(textureCoord.y * heatmapTexture.height);

        // Update heatmap texture pixel color with desired intensity
        heatmapTexture.SetPixel(x, y, Color.Lerp(heatmapTexture.GetPixel(x, y), color, intensity));
        heatmapTexture.Apply();
    }
}
