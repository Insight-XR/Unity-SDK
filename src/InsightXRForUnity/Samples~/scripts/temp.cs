using UnityEngine;

public class TestTemperature : MonoBehaviour
{
    public Material thermalMaterial;
    public float temperature = 50f;

    void Start()
    {
        if (thermalMaterial != null)
        {
            thermalMaterial.SetFloat("_Temperature", temperature);
            Debug.Log($"Initial temperature set to {temperature}");
        }
    }
}
