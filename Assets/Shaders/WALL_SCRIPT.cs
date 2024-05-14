using UnityEngine;

public class ShaderController : MonoBehaviour
{
    // Reference to the material using the custom shader
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        // Check if the material is assigned
        if (material == null)
        {
            Debug.LogError("Material not assigned!");
            return;
        }

        // Set properties for the first circle
        material.SetColor("_FirstCircleColor", Color.red); // Example color
        material.SetFloat("_FirstCircleRadius", 0f); // Example radius
        material.SetFloat("_FirstCircleBorder", 0.2f); // Example border

        // Set properties for the second circle
        material.SetColor("_SecondCircleColor", Color.green); // Example color
        material.SetFloat("_SecondCircleRadius", 0f); // Example radius
        material.SetFloat("_SecondCircleBorder", 0.4f); // Example border

        // Set properties for the third circle
        material.SetColor("_ThirdCircleColor", Color.blue); // Example color
        material.SetFloat("_ThirdCircleRadius", 0f); // Example radius
        material.SetFloat("_ThirdCircleBorder", 0.6f); // Example border

        // Set properties for the center positions (if needed)
        material.SetVector("_CenterPosition", new Vector4(0, 0, 0, 0)); // Example center position
        material.SetVector("_CenterPosition1", new Vector4(0, 1, 0, 0)); // Example center position
        // Repeat for _CenterPosition2, _CenterPosition3, _CenterPosition4 if needed
    }

    // Update is called once per frame
    void Update()
    {
        // You can update properties dynamically here if needed
    }
}
