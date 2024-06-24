using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript1 : MonoBehaviour
{
    float[] mPoints;
    int mHitCount;

    void Start()
    {
        mPoints = new float[32 * 3]; // 32 points
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
                AddHitPoint(hitMaterial, hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
            }
        }
    }

    public void AddHitPoint(Material material, float xp, float yp)
    {
        mPoints[mHitCount * 3] = xp;
        mPoints[mHitCount * 3 + 1] = yp;
        mPoints[mHitCount * 3 + 2] = Random.Range(1f, 3f);

        mHitCount++;
        mHitCount %= 32;

        material.SetFloatArray("_Hits", mPoints);
        material.SetInt("_HitCount", mHitCount);
    }
}
