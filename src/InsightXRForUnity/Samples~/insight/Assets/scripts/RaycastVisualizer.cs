using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript : MonoBehaviour
{
  private Dictionary<Material, List<Vector3>> materialHitPoints; // Store hit points for each material

  void Start()
  {
    materialHitPoints = new Dictionary<Material, List<Vector3>>();
  }

  void Update()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    Debug.DrawRay(ray.origin, ray.direction * 10, Color.green);

    if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("HeatMapLayer")))
    {
      Debug.Log("Hit Object " + hit.collider.gameObject.name);
      Debug.Log("Hit Texture coordinates = " + hit.textureCoord.x + "," + hit.textureCoord.y);

      // Get the MeshRenderer of the hit object
      MeshRenderer hitRenderer = hit.collider.GetComponent<MeshRenderer>();
      if (hitRenderer != null)
      {
        Material hitMaterial = hitRenderer.material;
        Vector3 hitPoint = new Vector3(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2, Random.Range(1f, 3f));

        // Add hit point to the list for this material
        if (!materialHitPoints.ContainsKey(hitMaterial))
        {
          materialHitPoints[hitMaterial] = new List<Vector3>(32);
        }
        materialHitPoints[hitMaterial].Add(hitPoint);

        // Update the material with the cumulative hit points
        UpdateMaterial(hitMaterial, materialHitPoints[hitMaterial]);
      }
    }
  }

  private void UpdateMaterial(Material material, List<Vector3> hitPoints)
  {
    float[] pointsArray = new float[32 * 3];
    int pointCount = Mathf.Min(hitPoints.Count, 32);
    for (int i = 0; i < pointCount; i++)
    {
      pointsArray[i * 3] = hitPoints[i].x;
      pointsArray[i * 3 + 1] = hitPoints[i].y;
      pointsArray[i * 3 + 2] = hitPoints[i].z;
    }

    material.SetFloatArray("_Hits", pointsArray);
    material.SetInt("_HitCount", pointCount);

    // If the list exceeds 32 points, remove the oldest ones
    if (hitPoints.Count > 32)
    {
      hitPoints.RemoveRange(0, hitPoints.Count - 32);
    }
  }
}
