using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientColorPlane : MonoBehaviour
{
    // Reference to the material of the plane
    public Material planeMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Get the mesh of the plane
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        // Get the vertices of the plane
        Vector3[] vertices = mesh.vertices;

        // Calculate the center of the plane
        Vector3 center = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }
        center /= vertices.Length;

        // Calculate the maximum distance from the center
        float maxDistance = 0f;
        foreach (Vector3 vertex in vertices)
        {
            float distance = Vector3.Distance(vertex, center);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        // Set the color gradient based on distance from the center
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(vertices[i], center);
            float t = distance / maxDistance; // Interpolation factor
            colors[i] = Color.Lerp(Color.red, Color.yellow, t);
        }

        // Set the colors to the material
        planeMaterial.SetColorArray("_Color", colors);
    }
}
