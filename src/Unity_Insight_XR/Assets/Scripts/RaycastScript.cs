using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastScript : MonoBehaviour
{
    private QuadScript1 quadScript;

    private void Start()
    {
        quadScript = GetComponent<QuadScript1>();
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

            // Pass the hit material and texture coordinates to the QuadScript1
            quadScript.AddHitPoint(hit.collider.GetComponent<MeshRenderer>().material, hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
        }
    }
}

