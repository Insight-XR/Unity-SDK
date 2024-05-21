using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript : MonoBehaviour
{
    private List<Vector3> mHitPoints;
    private int mHitCount;

    void Start()
    {
        mHitPoints = new List<Vector3>(32);
    }

    void Update()
    {
        // Create a ray from the camera's position, projecting forward
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
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
                Vector3 hitPoint = new Vector3(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2, Random.Range(1f, 3f));
                mHitPoints.Add(hitPoint);

                // Keep the list to a maximum of 32 points
                if (mHitPoints.Count > 32)
                {
                    mHitPoints.RemoveAt(0); // Remove the oldest point to maintain a fixed size
                }

                UpdateMaterial(hitRenderer.material);
            }
        }
    }

    private void UpdateMaterial(Material material)
    {
        float[] pointsArray = new float[32 * 3];
        int pointCount = Mathf.Min(mHitPoints.Count, 32);
        for (int i = 0; i < pointCount; i++)
        {
            pointsArray[i * 3] = mHitPoints[i].x;
            pointsArray[i * 3 + 1] = mHitPoints[i].y;
            pointsArray[i * 3 + 2] = mHitPoints[i].z;
        }

        material.SetFloatArray("_Hits", pointsArray);
        material.SetInt("_HitCount", pointCount);
    }
}
